#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Character.h"
#include "EldervaleCharacter.generated.h"

UCLASS()
class ELDERVALE_API AEldervaleCharacter : public ACharacter
{
    GENERATED_BODY()

public:
    AEldervaleCharacter();
    virtual void SetupPlayerInputComponent(UInputComponent* PlayerInputComponent) override;
    virtual void Tick(float DeltaSeconds) override;

protected:
    virtual void BeginPlay() override;
    void MoveForward(float Value);
    void MoveRight(float Value);
    void TurnCamera(float Value);
    void LookCamera(float Value);
    void Attack();
    void Skill();
    void Dodge();
    void BlockPressed();
    void BlockReleased();

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category="Camera")
    class USpringArmComponent* CameraBoom;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category="Camera")
    class UCameraComponent* FollowCamera;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Combat")
    float AttackRange = 280.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Combat")
    float SkillRange = 520.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Combat")
    float SkillCooldownSeconds = 4.0f;

    float SkillReadyAt = 0.0f;
    float DodgeReadyAt = 0.0f;
    bool bIsBlocking = false;
};
