#pragma once

#include "CoreMinimal.h"
#include "GameFramework/GameModeBase.h"
#include "EldervaleGameMode.generated.h"

UCLASS()
class ELDERVALE_API AEldervaleGameMode : public AGameModeBase
{
    GENERATED_BODY()
public:
    AEldervaleGameMode();
    virtual void BeginPlay() override;
};
