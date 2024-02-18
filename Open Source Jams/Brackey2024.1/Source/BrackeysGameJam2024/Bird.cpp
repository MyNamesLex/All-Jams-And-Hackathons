// Fill out your copyright notice in the Description page of Project Settings.


#include "Bird.h"

// Sets default values
ABird::ABird()
{
 	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

}

// Called when the game starts or when spawned
void ABird::BeginPlay()
{
	Super::BeginPlay();
	PlayIdleAnim();
}

// Called every frame
void ABird::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

	if (StartMoving == true)
	{
		MoveToLocation(RandomReachableLocationFVector);
	}
}

void ABird::FlyAway()
{
	PlayTakeoffAnim();
	GetWorldTimerManager().SetTimer(FlyDelayTimerHandle, this, &ABird::FlyToRandomReachableLocation, TimeUntilFlyAway, false, TimeUntilFlyAway);
}

void ABird::MoveToLocation(FVector RandomReachableLocation)
{
	if (floor(GetActorLocation().X) == floor(RandomReachableLocation.X) && floor(GetActorLocation().Y) == floor(RandomReachableLocation.Y) && floor(GetActorLocation().Z) == floor(RandomReachableLocation.Z))
	{
		Destroy();
	}
	else
	{
		//GEngine->AddOnScreenDebugMessage(-1, 3, FColor::Red, TEXT("MoveToLocation"));
		FVector Dir = RandomReachableLocation - GetActorLocation();
		Dir.Normalize();

		FVector Move = Dir * Speed * GetWorld()->DeltaTimeSeconds;

		SetActorLocation(GetActorLocation() + Move);

		if (StartMoving == false)
		{
			StartMoving = true;
		}
	}
}