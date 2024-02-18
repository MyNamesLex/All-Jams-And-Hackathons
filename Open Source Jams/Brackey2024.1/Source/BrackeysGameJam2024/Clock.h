// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "Components/TextRenderComponent.h"
#include "Clock.generated.h"

UCLASS()
class BRACKEYSGAMEJAM2024_API AClock : public AActor
{
	GENERATED_BODY()

public:
	// Sets default values for this actor's properties
	AClock();

protected:
	// Called when the game starts or when spawned
	virtual void BeginPlay() override;

public:
	// Called every frame
	virtual void Tick(float DeltaTime) override;

	UPROPERTY(EditAnywhere, BlueprintReadWrite)
		FString DateString;
	UPROPERTY(EditAnywhere, BlueprintReadWrite)
		FString TimeString;

	UFUNCTION(BlueprintCallable, BlueprintImplementableEvent)
		void UpdateText();
};
