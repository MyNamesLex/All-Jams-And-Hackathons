// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "LeverOpenInterface.h"
#include "RemoveableDownGravity.generated.h"

UCLASS()
class BRACKEYSGAMEJAM2024_API ARemoveableDownGravity : public AActor, public ILeverOpenInterface
{
	GENERATED_BODY()

public:
	// Sets default values for this actor's properties
	ARemoveableDownGravity();

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

public:
	// Called every frame
	virtual void Tick(float DeltaTime) override;

	UFUNCTION(BlueprintCallable, Category = "Open")
		virtual void LeverOpen() override;

	UFUNCTION(BlueprintCallable, BlueprintImplementableEvent)
		void BreakAllRemoveableDownGravityWalls();
};
