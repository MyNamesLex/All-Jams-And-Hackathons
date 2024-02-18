// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Character.h"
#include "PlayerMovement.generated.h"

UCLASS()
class BRACKEYSGAMEJAM2024_API APlayerMovement : public ACharacter
{
	GENERATED_BODY()

public:
	// Sets default values for this character's properties
	APlayerMovement();

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

public:
	// Called every frame
	virtual void Tick(float DeltaTime) override;

	// Called to bind functionality to input
	virtual void SetupPlayerInputComponent(class UInputComponent* PlayerInputComponent) override;

	void MoveForward(float axisVal);

	void MoveRight(float axisVal);

	void LeftClick();

	void Jump();

	UFUNCTION(BlueprintCallable)
		void Sprint();

	UFUNCTION(BlueprintCallable)
		void Walk();

	void HintKey();

	void PauseKey();

	void LookRight(float axisVal);

	void LookUp(float axisVal);

	void ResetButton();

	UPROPERTY(BlueprintReadWrite)
		float SprintSpeed = 900;
	UPROPERTY(BlueprintReadWrite)
		float WalkSpeed = 600;
	UPROPERTY(BlueprintReadWrite)
		float PlayerReach = 300;
	UPROPERTY(BlueprintReadWrite)
		float ShowTipTime = 15;
	UPROPERTY(BlueprintReadWrite)
		float TimeLocal = 0;
	UPROPERTY(BlueprintReadWrite)
		bool isShowingTip = false;
	UPROPERTY(BlueprintReadWrite)
		bool PressedShowHintKey = false;
	UPROPERTY(BlueprintReadWrite)
		bool LockPlayerCamera = false;

	bool ClickedResetButton = false;

	bool isPaused = false;

	UPROPERTY(BlueprintReadWrite)
		APlayerCameraManager* PlayerCamera;

	UFUNCTION(BlueprintCallable, BlueprintImplementableEvent)
		void ShowTip();

	UFUNCTION(BlueprintCallable, BlueprintImplementableEvent)
		void HideTip();

	UFUNCTION(BlueprintCallable, BlueprintImplementableEvent)
		void ShowTipKey();

	UFUNCTION(BlueprintCallable, BlueprintImplementableEvent)
		void HideTipKey();

	UFUNCTION(BlueprintCallable, BlueprintImplementableEvent)
		void PlayResetButtonSFX(FVector ButtonLocation);

	UFUNCTION(BlueprintCallable, BlueprintImplementableEvent)
		void ShowPauseMenu();
	UFUNCTION(BlueprintCallable, BlueprintImplementableEvent)
		void HidePauseMenu();

	FTimerHandle ResetButtonDelay;
};
