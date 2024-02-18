// Fill out your copyright notice in the Description page of Project Settings.

#include "Lever.h"
#include "LeverOpenInterface.h"

// Sets default values
ALever::ALever()
{
 	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

}

// Called when the game starts or when spawned
void ALever::BeginPlay()
{
	Super::BeginPlay();

}

// Called every frame
void ALever::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

}

void ALever::LeverOpen()
{
	ILeverOpenInterface* OpenThing = Cast<ILeverOpenInterface>(ThingToOpen);

	PlayLeverDownAnim();

	if (OpenThing)
	{
		OpenThing->LeverOpen();
	}
	else
	{
		GEngine->AddOnScreenDebugMessage(-1, 3, FColor::Blue, TEXT("ALever Open ThingToOpen Does Not Inherit Lever Interface"));
	}
}