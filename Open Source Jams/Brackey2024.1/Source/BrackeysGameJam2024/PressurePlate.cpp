// Fill out your copyright notice in the Description page of Project Settings.

#include "PressurePlate.h"
#include "PressurePlateInterface.h"

// Sets default values
APressurePlate::APressurePlate()
{
 	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

}

// Called when the game starts or when spawned
void APressurePlate::BeginPlay()
{
	Super::BeginPlay();
}

// Called every frame
void APressurePlate::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

}

void APressurePlate::PressurePlateOpen()
{
	IPressurePlateInterface* OpenThing = Cast<IPressurePlateInterface>(ThingToOpen);

	PlayPressurePlateDownAnim();

	if (OpenThing)
	{
		OpenThing->PressurePlateOpen();
	}
	else
	{
		GEngine->AddOnScreenDebugMessage(-1, 3, FColor::Blue, TEXT("APressurePlate Open ThingToOpen Does Not Inherit Pressure Plate Interface"));
	}
}
