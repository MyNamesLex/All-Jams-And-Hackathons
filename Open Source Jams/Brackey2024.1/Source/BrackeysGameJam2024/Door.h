// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "PressurePlateInterface.h"
#include "Door.generated.h"

UCLASS()
class BRACKEYSGAMEJAM2024_API ADoor : public AActor, public IPressurePlateInterface
{
	GENERATED_BODY()

public:
	// Sets default values for this actor's properties
	ADoor();

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

public:
	// Called every frame
	virtual void Tick(float DeltaTime) override;

	UFUNCTION(BlueprintCallable, Category = "Open")
		void OpenDoor();

	UFUNCTION(BlueprintCallable, Category = "Open")
		virtual void PressurePlateOpen() override;

	UFUNCTION(BlueprintImplementableEvent, BlueprintCallable)
		void PlayOpenDoorAnim();
};
