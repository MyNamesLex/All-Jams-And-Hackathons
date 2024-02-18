// Fill out your copyright notice in the Description page of Project Settings.

#include "Clock.h"
#include "CoreMinimal.h"

// Sets default values
AClock::AClock()
{
 	// Set this actor to call Tick() every frame.  You can turn this off to improve performance if you don't need it.
	PrimaryActorTick.bCanEverTick = true;

}

// Called when the game starts or when spawned
void AClock::BeginPlay()
{
	Super::BeginPlay();

}

// Called every frame
void AClock::Tick(float DeltaTime)
{
    FDateTime CurrentDateTime = FDateTime::Now();

    int Year = CurrentDateTime.GetYear();
    int Month = CurrentDateTime.GetMonth();
    int Day = CurrentDateTime.GetDay();
    int Hour = CurrentDateTime.GetHour();
    int Minute = CurrentDateTime.GetMinute();
    int Second = CurrentDateTime.GetSecond();

    FString DayString = FString::FromInt(Day);
    FString MonthString = FString::FromInt(Month);
    FString YearString = FString::FromInt(Year);
    FString HourString = FString::FromInt(Hour);
    FString MinuteString = FString::FromInt(Minute);
    FString SecondString = FString::FromInt(Second);

    DateString = FString::Printf(TEXT("%d-%02d-%02d"), Year, Month, Day);

    TimeString = FString::Printf(TEXT("%02d: %02d : %02d"), Hour, Minute, Second);

    UpdateText();
}

