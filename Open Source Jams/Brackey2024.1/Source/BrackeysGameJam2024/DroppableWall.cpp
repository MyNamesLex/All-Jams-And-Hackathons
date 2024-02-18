// Fill out your copyright notice in the Description page of Project Settings.


#include "DroppableWall.h"

// Sets default values
ADroppableWall::ADroppableWall()
{
 	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

}

// Called when the game starts or when spawned
void ADroppableWall::BeginPlay()
{
	Super::BeginPlay();

}

// Called every frame
void ADroppableWall::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);
}

void ADroppableWall::LeverOpen()
{
	WallBreakVFX(true);
}