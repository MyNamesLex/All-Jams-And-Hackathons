#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "LeverOpenInterface.h"
#include "Lever.generated.h"

UCLASS()
class BRACKEYSGAMEJAM2024_API ALever : public AActor, public ILeverOpenInterface
{
    GENERATED_BODY()

public:
    // Sets default values for this actor's properties
    ALever();

protected:
    // Called when the game starts or when spawned
    virtual void BeginPlay() override;

public:
    // Called every frame
    virtual void Tick(float DeltaTime) override;

    UFUNCTION(BlueprintCallable, Category = "Open")
        virtual void LeverOpen() override;

    UPROPERTY(EditAnywhere, BlueprintReadWrite)
        AActor* ThingToOpen;

    UFUNCTION(BlueprintImplementableEvent, BlueprintCallable)
        void PlayLeverDownAnim();
};