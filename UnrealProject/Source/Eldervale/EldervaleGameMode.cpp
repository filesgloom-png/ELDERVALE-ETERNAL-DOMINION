#include "EldervaleGameMode.h"
#include "EldervaleCharacter.h"
#include "EldervaleEnemy.h"
#include "Engine/StaticMeshActor.h"
#include "GameFramework/PlayerController.h"

#include "Components/StaticMeshComponent.h"
#include "UObject/ConstructorHelpers.h"
#include "GameFramework/PlayerStart.h"
#include "Engine/World.h"

AEldervaleGameMode::AEldervaleGameMode()
{
    DefaultPawnClass = AEldervaleCharacter::StaticClass();
    PlayerControllerClass = APlayerController::StaticClass();
}

void AEldervaleGameMode::BeginPlay()
{
    Super::BeginPlay();
    UWorld* World = GetWorld();
    if (!World) return;

    UStaticMesh* CubeMesh = LoadObject<UStaticMesh>(nullptr, TEXT("/Engine/BasicShapes/Cube.Cube"));
    if (CubeMesh)
    {
        AStaticMeshActor* Floor = World->SpawnActor<AStaticMeshActor>(FVector(0, 0, -60), FRotator::ZeroRotator);
        if (Floor)
        {
            Floor->GetStaticMeshComponent()->SetStaticMesh(CubeMesh);
            Floor->SetActorScale3D(FVector(35, 35, 1));
            Floor->GetStaticMeshComponent()->SetMobility(EComponentMobility::Static);
        }
        for (int32 Index = 0; Index < 20; ++Index)
        {
            const float Angle = (2.0f * PI * Index) / 20.0f;
            const FVector Position(FMath::Cos(Angle) * 1200.0f, FMath::Sin(Angle) * 1200.0f, 120.0f);
            AStaticMeshActor* Ruin = World->SpawnActor<AStaticMeshActor>(Position, FRotator(0, Index * 17.0f, Index % 3 == 0 ? 7.0f : 0.0f));
            if (Ruin)
            {
                Ruin->GetStaticMeshComponent()->SetStaticMesh(CubeMesh);
                Ruin->SetActorScale3D(FVector(1.4f, 1.2f, 2.4f + (Index % 4) * 0.5f));
            }
        }
    }
    World->SpawnActor<AEldervaleEnemy>(FVector(300, 0, 100), FRotator::ZeroRotator);
}
