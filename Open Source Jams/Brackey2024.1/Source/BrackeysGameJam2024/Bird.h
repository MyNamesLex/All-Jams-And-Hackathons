// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "Bird.generated.h"

UCLASS()
class BRACKEYSGAMEJAM2024_API ABird : public AActor
{
	GENERATED_BODY()

public:
	// Sets default values for this actor's properties
	ABird();

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

public:
	// Called every frame
	virtual void Tick(float DeltaTime) override;

	UFUNCTION(BlueprintCallable)
		void FlyAway();

	UFUNCTION(BlueprintImplementableEvent, BlueprintCallable)
		void PlayTakeoffAnim();

	UFUNCTION(BlueprintImplementableEvent, BlueprintCallable)
		void PlayIdleAnim();

	UFUNCTION(BlueprintImplementableEvent, BlueprintCallable)
		void PlayNormalFlyAnim();

	UFUNCTION(BlueprintImplementableEvent, BlueprintCallable)
		void FlyToRandomReachableLocation();

	UFUNCTION(BlueprintCallable)
		void MoveToLocation(FVector RandomReachableLocationParam);

	FTimerHandle FlyDelayTimerHandle;

	FTimerHandle ChangeToNormalFlyHandle;

	float TimeUntilFlyAway = 3;

	UPROPERTY(BlueprintReadOnly)
		float TimeUntilNormalFly = 3;

	UPROPERTY(BlueprintReadOnly)
		float FlyToRadius = 30000;

	UPROPERTY(BlueprintReadOnly)
		float Speed = 200;

	UPROPERTY(BlueprintReadOnly)
		bool StartMoving = false;

	UPROPERTY(BlueprintReadWrite)
		FVector RandomReachableLocationFVector;

	UPROPERTY(BlueprintReadWrite)
		bool AlreadyScared = false;

	UPROPERTY(BlueprintReadOnly)
		float HeightLocationIncrease = 400;
};
