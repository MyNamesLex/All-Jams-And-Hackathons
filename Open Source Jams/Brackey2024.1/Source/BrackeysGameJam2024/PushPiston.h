// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "PushPiston.generated.h"

UCLASS()
class BRACKEYSGAMEJAM2024_API APushPiston : public AActor
{
	GENERATED_BODY()

public:
	// Sets default values for this actor's properties
	APushPiston();

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

public:
	// Called every frame
	virtual void Tick(float DeltaTime) override;

	UPROPERTY(EditAnywhere, BlueprintReadWrite)
		float PistonForce;
};
