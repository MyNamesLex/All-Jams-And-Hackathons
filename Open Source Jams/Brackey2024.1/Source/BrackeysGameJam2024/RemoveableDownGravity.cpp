// Fill out your copyright notice in the Description page of Project Settings.


#include "RemoveableDownGravity.h"

// Sets default values
ARemoveableDownGravity::ARemoveableDownGravity()
{
 	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

}

// Called when the game starts or when spawned
void ARemoveableDownGravity::BeginPlay()
{
	Super::BeginPlay();

}

// Called every frame
void ARemoveableDownGravity::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

}

void ARemoveableDownGravity::LeverOpen()
{
	BreakAllRemoveableDownGravityWalls();
}

