using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EldervaleBootstrap : MonoBehaviour
{
    const float WalkSpeed = 5.2f;
    GameObject player, enemy, enemyVisual;
    CharacterController controller;
    Camera cam;
    Text status, hpText;
    int playerHp = 100, enemyHp = 100;
    float skillCooldown, dodgeTime;
    Vector2 moveInput;
    int joystickFinger = -1;
    Vector2 joystickStart;
    Rect joystickRect = new Rect(35, Screen.height - 205, 170, 170);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        var root = new GameObject("ELDERVALE Runtime");
        root.AddComponent<EldervaleBootstrap>();
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        BuildWorld();
        BuildPlayer();
        BuildEnemy();
        BuildCamera();
        BuildHud();
    }

    void Update()
    {
        skillCooldown = Mathf.Max(0, skillCooldown - Time.deltaTime);
        dodgeTime = Mathf.Max(0, dodgeTime - Time.deltaTime);
        Vector2 input = moveInput;
        input += new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input = Vector2.ClampMagnitude(input, 1);
        Vector3 forward = cam.transform.forward; forward.y = 0; forward.Normalize();
        Vector3 right = cam.transform.right; right.y = 0; right.Normalize();
        Vector3 dir = right * input.x + forward * input.y;
        if (dodgeTime > 0) dir = player.transform.forward * 1.7f;
        controller.Move((dir * (dodgeTime > 0 ? 11f : WalkSpeed) + Vector3.down * 9.8f) * Time.deltaTime);
        if (dir.sqrMagnitude > .02f) player.transform.rotation = Quaternion.Slerp(player.transform.rotation, Quaternion.LookRotation(dir), 12 * Time.deltaTime);
        cam.transform.position = player.transform.position + player.transform.rotation * new Vector3(0, 3.2f, -6.5f);
        cam.transform.LookAt(player.transform.position + Vector3.up * 1.1f);
        if (enemyHp > 0 && enemy) enemy.transform.LookAt(new Vector3(player.transform.position.x, enemy.transform.position.y, player.transform.position.z));
        HandleTouchJoystick();
        if (hpText) hpText.text = "HP " + playerHp + "/100     WRAITH " + enemyHp + "/100";
    }

    void SetColor(Renderer renderer, Color color) { var shader=Shader.Find("Universal Render Pipeline/Lit"); if(shader==null) shader=Shader.Find("Standard"); renderer.material.shader=shader; renderer.material.color=color; }

    void BuildWorld()
    {
        RenderSettings.ambientLight = new Color(.38f, .42f, .52f);
        RenderSettings.fog = true; RenderSettings.fogColor = new Color(.10f, .13f, .18f); RenderSettings.fogDensity = .008f;
        var light = new GameObject("Moonlight").AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.35f; light.transform.rotation = Quaternion.Euler(48, -32, 0); light.shadows = LightShadows.Soft;
        Cube("Ancient Arena", Vector3.down * .55f, new Vector3(70, 1, 70), new Color(.19f,.23f,.22f));
        for (int i=0;i<32;i++) { float a=i*Mathf.PI*2/32f; float radius=12+(i%4)*1.1f; float h=2.5f+(i%5)*.65f; var ruin=Cube("Broken Ruin",new Vector3(Mathf.Cos(a)*radius,h*.5f-.2f,Mathf.Sin(a)*radius),new Vector3(1.1f,h,1),new Color(.25f,.29f,.31f)); ruin.transform.rotation=Quaternion.Euler(0,i*19, i%3*4); }
        for(int i=0;i<14;i++) Cube("Mossy Stone",new Vector3((i*7)%21-10,-.02f,(i*11)%23-11),new Vector3(.7f,.16f,.7f),new Color(.26f,.33f,.27f));
        Cube("Obelisk",new Vector3(0,2.2f,-8),new Vector3(1,4.4f,1),new Color(.08f,.17f,.28f));
        var glow=new GameObject("Arcane Glow").AddComponent<Light>(); glow.type=LightType.Point; glow.color=new Color(.12f,.42f,1); glow.intensity=5; glow.range=12; glow.transform.position=new Vector3(0,3,-7);
    }

    GameObject Cube(string name, Vector3 pos, Vector3 scale, Color color)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=name; go.transform.position=pos; go.transform.localScale=scale;
        var shader=Shader.Find("Universal Render Pipeline/Lit"); if(shader==null) shader=Shader.Find("Standard"); var mat=new Material(shader); mat.color=color; if(mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic",.18f); if(mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness",.38f); go.GetComponent<Renderer>().material=mat; return go;
    }

    void BuildPlayer()
    {
        player=new GameObject("Player"); player.transform.position=new Vector3(0,1,4);
        controller=player.AddComponent<CharacterController>(); controller.height=1.8f; controller.radius=.42f; controller.center=Vector3.up*.9f;
        var body=GameObject.CreatePrimitive(PrimitiveType.Capsule); body.name="Armored Adventurer"; body.transform.SetParent(player.transform,false); body.transform.localPosition=Vector3.up*.9f; body.transform.localScale=new Vector3(.78f,.95f,.78f); body.GetComponent<Collider>().enabled=false; SetColor(body.GetComponent<Renderer>(),new Color(.15f,.28f,.48f));
        var head=GameObject.CreatePrimitive(PrimitiveType.Sphere); head.transform.SetParent(player.transform,false); head.transform.localPosition=Vector3.up*1.92f; head.transform.localScale=Vector3.one*.48f; head.GetComponent<Collider>().enabled=false; SetColor(head.GetComponent<Renderer>(),new Color(.76f,.59f,.44f));
        var blade=Cube("Runeblade",player.transform.position+new Vector3(.48f,1,-.35f),new Vector3(.09f,.95f,.13f),new Color(.48f,.78f,1)); blade.transform.SetParent(player.transform); blade.transform.localPosition=new Vector3(.48f,1,-.35f); blade.transform.localRotation=Quaternion.Euler(-18,0,0);
    }

    void BuildEnemy()
    {
        enemy=new GameObject("Training Wraith"); enemy.transform.position=new Vector3(0,.95f,-3);
        var col=enemy.AddComponent<CapsuleCollider>(); col.height=1.9f; col.radius=.58f;
        enemyVisual=GameObject.CreatePrimitive(PrimitiveType.Capsule); enemyVisual.transform.SetParent(enemy.transform,false); enemyVisual.transform.localPosition=Vector3.up*.95f; enemyVisual.GetComponent<Collider>().enabled=false; SetColor(enemyVisual.GetComponent<Renderer>(),new Color(.42f,.10f,.18f));
        var eye=GameObject.CreatePrimitive(PrimitiveType.Sphere); eye.transform.SetParent(enemy.transform,false); eye.transform.localPosition=new Vector3(0,1.35f,.49f); eye.transform.localScale=Vector3.one*.24f; eye.GetComponent<Collider>().enabled=false; SetColor(eye.GetComponent<Renderer>(),new Color(1,.08f,.02f));
    }

    void BuildCamera()
    {
        var go=new GameObject("Third Person Camera"); cam=go.AddComponent<Camera>(); cam.fieldOfView=65; go.transform.position=player.transform.position+new Vector3(0,3.2f,-6.5f); go.transform.LookAt(player.transform.position+Vector3.up);
    }

    void BuildHud()
    {
        var canvasGo=new GameObject("Mobile HUD"); var canvas=canvasGo.AddComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvasGo.AddComponent<CanvasScaler>().uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; canvasGo.GetComponent<CanvasScaler>().referenceResolution=new Vector2(1280,720); canvasGo.AddComponent<GraphicRaycaster>();
        hpText=MakeText(canvas.transform,"HP 100/100     WRAITH 100/100",new Vector2(22,-20),new Vector2(500,42),22,TextAnchor.MiddleLeft);
        status=MakeText(canvas.transform,"SHATTERED REACH  •  MANUAL COMBAT ONLY",new Vector2(22,-62),new Vector2(580,36),17,TextAnchor.MiddleLeft);
        MakeButton(canvas.transform,"ATTACK",new Vector2(-180,90),new Vector2(145,76),Attack);
        MakeButton(canvas.transform,"SKILL",new Vector2(-292,174),new Vector2(112,62),Skill);
        MakeButton(canvas.transform,"DODGE",new Vector2(-320,92),new Vector2(112,62),Dodge);
        MakeButton(canvas.transform,"BLOCK",new Vector2(-180,174),new Vector2(112,62),Block);
        MakeText(canvas.transform,"MOVE",new Vector2(45,-Screen.height+15),new Vector2(150,30),15,TextAnchor.MiddleCenter);
    }

    Text MakeText(Transform parent,string value,Vector2 anchored,Vector2 size,int font,TextAnchor align)
    {
        var go=new GameObject(value); go.transform.SetParent(parent,false); var rect=go.AddComponent<RectTransform>(); rect.anchorMin=new Vector2(0,1); rect.anchorMax=new Vector2(0,1); rect.pivot=new Vector2(0,1); rect.anchoredPosition=anchored; rect.sizeDelta=size;
        var t=go.AddComponent<Text>(); t.text=value; t.font=Resources.GetBuiltinResource<Font>("Arial.ttf"); t.fontSize=font; t.color=Color.white; t.alignment=align; t.horizontalOverflow=HorizontalWrapMode.Overflow; return t;
    }

    void MakeButton(Transform parent,string label,Vector2 offset,Vector2 size,UnityEngine.Events.UnityAction action)
    {
        var go=new GameObject(label); go.transform.SetParent(parent,false); var rect=go.AddComponent<RectTransform>(); rect.anchorMin=new Vector2(1,0); rect.anchorMax=new Vector2(1,0); rect.pivot=new Vector2(1,0); rect.anchoredPosition=offset; rect.sizeDelta=size;
        var image=go.AddComponent<Image>(); image.color=new Color(.07f,.16f,.29f,.94f); var button=go.AddComponent<Button>(); button.targetGraphic=image; button.onClick.AddListener(action);
        var textGo=new GameObject("Label"); textGo.transform.SetParent(go.transform,false); var tr=textGo.AddComponent<RectTransform>(); tr.anchorMin=Vector2.zero; tr.anchorMax=Vector2.one; tr.offsetMin=Vector2.zero; tr.offsetMax=Vector2.zero;
        var txt=textGo.AddComponent<Text>(); txt.font=Resources.GetBuiltinResource<Font>("Arial.ttf"); txt.text=label; txt.fontSize=19; txt.fontStyle=FontStyle.Bold; txt.alignment=TextAnchor.MiddleCenter; txt.color=Color.white;
    }

    void HandleTouchJoystick()
    {
        if(Input.touchCount==0) { if(joystickFinger!=-1) { joystickFinger=-1; moveInput=Vector2.zero; } return; }
        foreach(Touch touch in Input.touches)
        {
            if(touch.phase==TouchPhase.Began && touch.position.x<Screen.width*.42f && touch.position.y<Screen.height*.55f && joystickFinger==-1) { joystickFinger=touch.fingerId; joystickStart=touch.position; }
            if(touch.fingerId==joystickFinger)
            {
                if(touch.phase==TouchPhase.Ended || touch.phase==TouchPhase.Canceled) { joystickFinger=-1; moveInput=Vector2.zero; }
                else { Vector2 delta=(touch.position-joystickStart)/85f; moveInput=Vector2.ClampMagnitude(delta,1); }
            }
        }
    }

    void Attack()
    {
        if(enemyHp<=0) { SetStatus("WRAITH DEFEATED"); return; }
        if(Vector3.Distance(player.transform.position,enemy.transform.position)>3.2f) { SetStatus("TOO FAR — MOVE CLOSER"); return; }
        enemyHp=Mathf.Max(0,enemyHp-24); SetStatus("SWORD STRIKE  •  -24 HP"); HitFeedback();
        if(enemyHp==0) { enemy.SetActive(false); SetStatus("WRAITH DEFEATED — NO AUTO-ATTACK"); }
    }
    void Skill()
    {
        if(skillCooldown>0) { SetStatus("SKILL COOLDOWN: "+skillCooldown.ToString("0.0")+" S"); return; }
        if(enemyHp<=0) { SetStatus("NO ACTIVE TARGET"); return; }
        if(Vector3.Distance(player.transform.position,enemy.transform.position)>5.5f) { SetStatus("SKILL OUT OF RANGE"); return; }
        enemyHp=Mathf.Max(0,enemyHp-42); skillCooldown=4; SetStatus("ARCANE SLASH  •  -42 HP"); HitFeedback(); if(enemyHp==0) { enemy.SetActive(false); SetStatus("WRAITH DEFEATED"); }
    }
    void Dodge() { dodgeTime=.24f; SetStatus("DODGE"); }
    void Block() { SetStatus("BLOCK — MANUAL DEFENSE"); }
    void SetStatus(string message) { if(status) status.text=message; }
    void HitFeedback() { if(enemyVisual) StartCoroutine(Flash()); }
    IEnumerator Flash() { var r=enemyVisual.GetComponent<Renderer>(); if(!r) yield break; Color old=r.material.color; r.material.color=Color.white; yield return new WaitForSeconds(.12f); if(r) r.material.color=old; }
}
