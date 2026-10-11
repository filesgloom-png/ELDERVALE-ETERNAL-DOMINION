#include "EldervaleEnemy.h"
#include "Components/StaticMeshComponent.h"
#include "UObject/ConstructorHelpers.h"
#include "Materials/MaterialInstanceDynamic.h"
#include "Engine/StaticMesh.h"

AEldervaleEnemy::AEldervaleEnemy()
{
    PrimaryActorTick.bCanEverTick = false;
    Tags.Add(FName("Enemy"));
    Body = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("WraithBody"));
    SetRootComponent(Body);
    static ConstructorHelpers::FObjectFinder<UStaticMesh> SphereMesh(TEXT("/Engine/BasicShapes/Sphere.Sphere"));
    if (SphereMesh.Succeeded()) Body->SetStaticMesh(SphereMesh.Object);
    Body->SetCollisionProfileName(TEXT("BlockAllDynamic"));
    Body->SetWorldScale3D(FVector(1.1f, 1.1f, 1.8f));
    OnTakeAnyDamage.AddDynamic(this, &AEldervaleEnemy::ReceiveDamage);
}

void AEldervaleEnemy::ReceiveDamage(AActor* DamagedActor, float Damage, const UDamageType* DamageType,
    AController* InstigatedBy, AActor* DamageCauser)
{
    Health = FMath::Max(0.0f, Health - Damage);
    if (Body)
    {
        if (UMaterialInstanceDynamic* Mat = Body->CreateAndSetMaterialInstanceDynamic(0))
        {
            Mat->SetVectorParameterValue(FName("BaseColor"), Health <= 0.0f ? FLinearColor::Black : FLinearColor(0.8f, 0.04f, 0.12f));
        }
    }
    if (Health <= 0.0f) Destroy();
}
