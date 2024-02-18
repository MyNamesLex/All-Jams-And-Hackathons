// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "PressurePlateInterface.h"
#include "PressurePlate.generated.h"

UCLASS()
class BRACKEYSGAMEJAM2024_API APressurePlate : public AActor, public IPressurePlateInterface
{
	GENERATED_BODY()

public:
	// Sets default values for this actor's properties
	APressurePlate();

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

public:
	// Called every frame
	virtual void Tick(float DeltaTime) override;

	UFUNCTION(BlueprintCallable, Category = "Collision")
		virtual void PressurePlateOpen() override;

	UPROPERTY(EditAnywhere, BlueprintReadWrite)
		AActor* ThingToOpen;

	UFUNCTION(BlueprintImplementableEvent, BlueprintCallable)
		void PlayPressurePlateDownAnim();
};
