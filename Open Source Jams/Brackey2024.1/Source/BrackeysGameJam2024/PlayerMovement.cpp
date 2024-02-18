// Fill out your copyright notice in the Description page of Project Settings.


#include "PlayerMovement.h"
#include "GameFramework/Controller.h"
#include "GameFramework/Character.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "CollisionQueryParams.h"
#include "Kismet/GameplayStatics.h"
#include "UObject/NameTypes.h"
#include "Lever.h"

// Sets default values
APlayerMovement::APlayerMovement()
{
 	// Set this character to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

}

// Called when the game starts or when spawned
void APlayerMovement::BeginPlay()
{
	Super::BeginPlay();
    Walk();
}

// Called every frame-+-
void APlayerMovement::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

    TimeLocal += DeltaTime;

    if (TimeLocal >= ShowTipTime && PressedShowHintKey == false && isShowingTip == false) // Time > ShowTipTime, Not Pressed Tip Key, Not Showing Tip
    {
        ShowTipKey();
    }

    if (isShowingTip == false && PressedShowHintKey == true) // Pressed Tip Key, Not Showing Tip, Show Tip and Hide Tip Key
    {
        ShowTip();
        HideTipKey();
    }

    if (isShowingTip == true && PressedShowHintKey == false) // Pressed Tip Key Twice, Showing Tip, Hide Tip and Show Tip Key
    {
        HideTip();
        ShowTipKey();
    }
}

// Called to bind functionality to input
void APlayerMovement::SetupPlayerInputComponent(UInputComponent* PlayerInputComponent)
{
	Super::SetupPlayerInputComponent(PlayerInputComponent);

    //ACTION
	PlayerInputComponent->BindAction("Jump", IE_Pressed, this, &APlayerMovement::Jump);
    PlayerInputComponent->BindAction("LeftClick", IE_Pressed, this, &APlayerMovement::LeftClick);
    PlayerInputComponent->BindAction("Shift", IE_Pressed, this, &APlayerMovement::Sprint);
    PlayerInputComponent->BindAction("Shift", IE_Released, this, &APlayerMovement::Walk);
    PlayerInputComponent->BindAction("HintKey", IE_Pressed, this, &APlayerMovement::HintKey);
    PlayerInputComponent->BindAction("PauseKey", IE_Pressed, this, &APlayerMovement::PauseKey);

    //AXIS
    PlayerInputComponent->BindAxis("Right", this, &APlayerMovement::MoveRight);
    PlayerInputComponent->BindAxis("Forward", this, &APlayerMovement::MoveForward);
    PlayerInputComponent->BindAxis("Vertical", this, &APlayerMovement::LookUp);
    PlayerInputComponent->BindAxis("Horizontal", this, &APlayerMovement::LookRight);
}

void APlayerMovement::PauseKey()
{
    if (isPaused == false)
    {
        isPaused = true;
        ShowPauseMenu();
    }
    else
    {
        isPaused = false;
        HidePauseMenu();
    }
}

void APlayerMovement::LookUp(float axisVal)
{
    if (LockPlayerCamera == false)
    {
        APlayerMovement::AddControllerPitchInput(axisVal);
    }
}

void APlayerMovement::LookRight(float axisVal)
{
    if (LockPlayerCamera == false)
    {
        APlayerMovement::AddControllerYawInput(axisVal);
    }
}

void APlayerMovement::MoveRight(float axisVal)
{
    AddMovementInput(GetActorRightVector() * axisVal);
}

void APlayerMovement::MoveForward(float axisVal)
{
    AddMovementInput(GetActorForwardVector() * axisVal);
}

void APlayerMovement::HintKey()
{
    if (PressedShowHintKey == true)
    {
        PressedShowHintKey = false;
    }
    else
    {
        PressedShowHintKey = true;
    }
}

void APlayerMovement::LeftClick()
{
    GEngine->AddOnScreenDebugMessage(-1, 3, FColor::Red, TEXT("CLICK"));

    FHitResult* Hit = new FHitResult();

    if (UWorld* World = GetWorld())
    {
        PlayerCamera = World->GetFirstPlayerController()->PlayerCameraManager;
    }

    FVector StartCast = PlayerCamera->GetCameraLocation();
    FVector Forward = PlayerCamera->GetActorForwardVector();
    FVector EndCast = (Forward * PlayerReach) + StartCast;
    FCollisionQueryParams* col = new FCollisionQueryParams();


    if (GetWorld()->LineTraceSingleByChannel(*Hit, StartCast, EndCast, ECC_Visibility, *col))
    {
        DrawDebugLine(GetWorld(), StartCast, EndCast, FColor(255, 0, 0), true);

        if(Hit->GetActor() != NULL)
        {
            GEngine->AddOnScreenDebugMessage(-1, 3, FColor::Red, TEXT("HIT SOMETHING"));

            if (Hit->GetActor()->Tags.Contains("Lever"))
            {
                GEngine->AddOnScreenDebugMessage(-1, 3, FColor::Red, TEXT("LEVER"));

                ALever* lever = Cast<ALever>(Hit->GetActor());
                lever->LeverOpen();
            }

            if (Hit->GetActor()->Tags.Contains("ResetLevel") && ClickedResetButton == false)
            {
                ClickedResetButton = true;
                PlayResetButtonSFX(Hit->GetActor()->GetActorLocation());
                GetWorldTimerManager().SetTimer(ResetButtonDelay, this, &APlayerMovement::ResetButton, 1.0f, false, 1.0f);
            }
        }
    }
}

void APlayerMovement::ResetButton()
{
    GEngine->AddOnScreenDebugMessage(-1, 3, FColor::Red, TEXT("RESET LEVEL"));
    UGameplayStatics::OpenLevel(GetWorld(), FName(*GetWorld()->GetName()), true);
}

void APlayerMovement::Jump()
{
    ACharacter::Jump();
}

void APlayerMovement::Sprint()
{
    UCharacterMovementComponent* charactermovement = GetCharacterMovement();
    charactermovement->MaxWalkSpeed = SprintSpeed;
}

void APlayerMovement::Walk()
{
    UCharacterMovementComponent* charactermovement = GetCharacterMovement();
    charactermovement->MaxWalkSpeed = WalkSpeed;
}