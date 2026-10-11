#include "EldervaleCharacter.h"
#include "Camera/CameraComponent.h"
#include "GameFramework/SpringArmComponent.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "Components/CapsuleComponent.h"
#include "Kismet/GameplayStatics.h"
#include "Engine/World.h"
#include "DrawDebugHelpers.h"

AEldervaleCharacter::AEldervaleCharacter()
{
    PrimaryActorTick.bCanEverTick = true;
    GetCapsuleComponent()->InitCapsuleSize(42.0f, 96.0f);

    CameraBoom = CreateDefaultSubobject<USpringArmComponent>(TEXT("CameraBoom"));
    CameraBoom->SetupAttachment(RootComponent);
    CameraBoom->TargetArmLength = 430.0f;
    CameraBoom->SetRelativeLocation(FVector(0.0f, 0.0f, 75.0f));
    CameraBoom->bUsePawnControlRotation = true;
    CameraBoom->bEnableCameraLag = true;
    CameraBoom->CameraLagSpeed = 8.0f;

    FollowCamera = CreateDefaultSubobject<UCameraComponent>(TEXT("FollowCamera"));
    FollowCamera->SetupAttachment(CameraBoom, USpringArmComponent::SocketName);
    FollowCamera->bUsePawnControlRotation = false;

    bUseControllerRotationYaw = false;
    GetCharacterMovement()->bOrientRotationToMovement = true;
    GetCharacterMovement()->RotationRate = FRotator(0.0f, 520.0f, 0.0f);
    GetCharacterMovement()->MaxWalkSpeed = 420.0f;
    GetCharacterMovement()->BrakingDecelerationWalking = 1700.0f;
}

void AEldervaleCharacter::BeginPlay()
{
    Super::BeginPlay();
}

void AEldervaleCharacter::Tick(float DeltaSeconds)
{
    Super::Tick(DeltaSeconds);
}

void AEldervaleCharacter::SetupPlayerInputComponent(UInputComponent* PlayerInputComponent)
{
    Super::SetupPlayerInputComponent(PlayerInputComponent);
    PlayerInputComponent->BindAxis("MoveForward", this, &AEldervaleCharacter::MoveForward);
    PlayerInputComponent->BindAxis("MoveRight", this, &AEldervaleCharacter::MoveRight);
    PlayerInputComponent->BindAxis("Turn", this, &AEldervaleCharacter::TurnCamera);
    PlayerInputComponent->BindAxis("LookUp", this, &AEldervaleCharacter::LookCamera);
    PlayerInputComponent->BindAction("Attack", IE_Pressed, this, &AEldervaleCharacter::Attack);
    PlayerInputComponent->BindAction("Skill", IE_Pressed, this, &AEldervaleCharacter::Skill);
    PlayerInputComponent->BindAction("Dodge", IE_Pressed, this, &AEldervaleCharacter::Dodge);
    PlayerInputComponent->BindAction("Block", IE_Pressed, this, &AEldervaleCharacter::BlockPressed);
    PlayerInputComponent->BindAction("Block", IE_Released, this, &AEldervaleCharacter::BlockReleased);
}

void AEldervaleCharacter::MoveForward(float Value)
{
    if (!Controller || FMath::IsNearlyZero(Value)) return;
    const FRotator YawRotation(0.0f, Controller->GetControlRotation().Yaw, 0.0f);
    AddMovementInput(FRotationMatrix(YawRotation).GetUnitAxis(EAxis::X), Value);
}

void AEldervaleCharacter::MoveRight(float Value)
{
    if (!Controller || FMath::IsNearlyZero(Value)) return;
    const FRotator YawRotation(0.0f, Controller->GetControlRotation().Yaw, 0.0f);
    AddMovementInput(FRotationMatrix(YawRotation).GetUnitAxis(EAxis::Y), Value);
}

void AEldervaleCharacter::TurnCamera(float Value) { AddControllerYawInput(Value); }
void AEldervaleCharacter::LookCamera(float Value) { AddControllerPitchInput(Value); }

void AEldervaleCharacter::Attack()
{
    FHitResult Hit;
    const FVector Start = FollowCamera->GetComponentLocation();
    const FVector End = Start + FollowCamera->GetForwardVector() * AttackRange;
    FCollisionQueryParams Params(SCENE_QUERY_STAT(EldervaleManualAttack), false, this);
    if (GetWorld()->LineTraceSingleByChannel(Hit, Start, End, ECC_Pawn, Params))
    {
        AActor* Target = Hit.GetActor();
        if (Target && Target->ActorHasTag(FName("Enemy")))
        {
            UGameplayStatics::ApplyDamage(Target, 24.0f, GetController(), this, UDamageType::StaticClass());
        }
    }
}

void AEldervaleCharacter::Skill()
{
    if (GetWorld()->GetTimeSeconds() < SkillReadyAt) return;
    SkillReadyAt = GetWorld()->GetTimeSeconds() + SkillCooldownSeconds;
    TArray<FHitResult> Hits;
    const FVector Start = FollowCamera->GetComponentLocation();
    const FVector End = Start + FollowCamera->GetForwardVector() * SkillRange;
    FCollisionQueryParams Params(SCENE_QUERY_STAT(EldervaleManualSkill), false, this);
    if (GetWorld()->LineTraceMultiByChannel(Hits, Start, End, ECC_Pawn, Params))
    {
        for (const FHitResult& Hit : Hits)
        {
            AActor* Target = Hit.GetActor();
            if (Target && Target->ActorHasTag(FName("Enemy")))
            {
                UGameplayStatics::ApplyDamage(Target, 42.0f, GetController(), this, UDamageType::StaticClass());
                break;
            }
        }
    }
}

void AEldervaleCharacter::Dodge()
{
    if (GetWorld()->GetTimeSeconds() < DodgeReadyAt) return;
    DodgeReadyAt = GetWorld()->GetTimeSeconds() + 0.8f;
    LaunchCharacter(GetActorForwardVector() * 720.0f + FVector(0.0f, 0.0f, 65.0f), true, false);
}

void AEldervaleCharacter::BlockPressed()
{
    bIsBlocking = true;
    GetCharacterMovement()->MaxWalkSpeed = 160.0f;
}

void AEldervaleCharacter::BlockReleased()
{
    bIsBlocking = false;
    GetCharacterMovement()->MaxWalkSpeed = 420.0f;
}
