using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;


public class BuildingProcessor : MonoBehaviour
{
    public bool isProcessing = false;

    public Action OnRecipeChanged;
    /*

    

    
    private void Update()
    {
        HandleRecipeSwitching();
        if (isProcessing && CanProcessRecipe())
        {
            StartCoroutine(ProcessRecipe());
        }
    }
    
    private void HandleRecipeSwitching()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SetRecipe((currentRecipeIndex + 1) % availableRecepies.Count);
        }else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SetRecipe((currentRecipeIndex - 1 + availableRecepies.Count) % availableRecepies.Count);
        }
    }

    private void SetRecipe(int index)
    {
        if (index < 0 || index >= availableRecepies.Count)
            return;
        currentRecipeIndex = index;
        Debug.Log($"Switched Recipe to {availableRecepies[currentRecipeIndex].name}");
        OnRecipeChanged?.Invoke();
    }

    private bool CanProcessRecipe()
    {
        

        RecipeData recipe = availableRecepies[currentRecipeIndex];
        if (recipe == null || recipe.inputItems.Count == 0)
            return false;

        
        return true;
    }

    private IEnumerator ProcessRecipe()
    {
        isProcessing = true;
        RecipeData recipe = availableRecepies[currentRecipeIndex];

        foreach (var  ingredient in recipe.inputItems)
        {
            //inventory.RemoveInputItems(ingredient.item, ingredient.amount);
        }

        yield return new WaitForSeconds(recipe.processingTime);

        //inventory.AddOutputItems(recipe.outputItem, recipe.outputAmount);

        isProcessing = false;
    }

    public RecipeData GetCurrentRecipe()
    {
        if (availableRecepies.Count == 0) return null;
        return availableRecepies[currentRecipeIndex];
    }

    public void SetIndex(int index)
    {
        this.index = index;
        buildingSaveData = new BuildingSaveData(availableRecepies, transform.position, transform.rotation, isProcessing, currentRecipeIndex, index);
    }*/
}

