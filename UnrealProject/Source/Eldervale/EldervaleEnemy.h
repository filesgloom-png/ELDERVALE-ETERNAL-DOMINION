#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "EldervaleEnemy.generated.h"

UCLASS()
class ELDERVALE_API AEldervaleEnemy : public AActor
{
    GENERATED_BODY()
public:
    AEldervaleEnemy();
protected:
    UPROPERTY(VisibleAnywhere)
    class UStaticMeshComponent* Body;
    UPROPERTY(EditAnywhere, Category="Combat")
    float MaxHealth = 100.0f;
    float Health = 100.0f;
    UFUNCTION()
    void ReceiveDamage(AActor* DamagedActor, float Damage, const class UDamageType* DamageType,
        class AController* InstigatedBy, AActor* DamageCauser);
};
