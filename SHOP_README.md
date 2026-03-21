# HumanFarm Shop System

This document describes the Shop System implementation for the HumanFarm Unity project.

## Features

- Buy Humans and Food using Z-Coins
- 5 different Human types from HumanSO
- 4 different Food types from FoodSO
- UI with scrollable item list
- Category switching between Humans and Foods

## Setup Instructions

### 1. Create the Shop UI Canvas

1. In your Main scene, create a new Canvas (UI > Canvas)
2. Set Canvas Scaler to Scale With Screen Size (Reference Resolution: 1920x1080)
3. Create a Panel as child of Canvas (UI > Panel) - name it "ShopPanel"
4. Add a Scroll View inside ShopPanel (UI > Scroll View)
   - Set Scroll View size to fill parent
   - Name the Viewport > Content as "ShopContent"
   - Set Content Vertical Layout Group (Add Component > Vertical Layout Group)
     - Spacing: 10
     - Child Alignment: Upper Center

### 2. Create Shop Item Prefab

1. Create a new UI Panel (UI > Panel) - name it "ShopItem"
2. Add child Image (UI > Image) - name "ItemImage" - for icon
3. Add child TextMeshPro (UI > TextMeshPro - Text) - name "ItemName" - for name
4. Add child TextMeshPro - name "CostText" - for cost
5. Add child Button (UI > Button) - name "BuyButton" - for buying
6. Attach ShopItemUI script to ShopItem
7. Assign the UI references in ShopItemUI inspector
8. Save as Prefab in Assets/Prefabs/ShopItem.prefab

### 3. Setup Shop Manager

1. Create empty GameObject "ShopManager"
2. Attach ShopManager script
3. Attach GameManager script to same object (if not already)
4. In ShopManager inspector:
   - Assign ShopContent transform to contentParent
   - Assign ShopItem prefab to itemPrefab
   - Create/assign a spawn point transform for humanSpawnPoint
   - Assign your HumanSO assets to humans array (5 items)
   - Assign your FoodSO assets to foods array (4 items)

### 4. Setup Shop UI Controller

1. Create empty GameObject "ShopUI"
2. Attach ShopUI script
3. In ShopUI inspector:
   - Assign ShopManager reference
   - Create two Buttons in ShopPanel: "HumansButton" and "FoodsButton"
   - Assign them to humansButton and foodsButton
   - Assign ShopPanel to shopPanel

### 5. Create Human and Food SO Assets

1. Create HumanSO assets: Assets > Create > Scriptable Objects > HumanSO
   - Fill name, profilePic, humanPrefab, costs, etc.
   - Create 5 different types

2. Create FoodSO assets: Assets > Create > Scriptable Objects > FoodSO
   - Fill name, foodIcon, foodPrefab, costs, etc.
   - Create 4 different types

### 6. Add Shop Toggle Button

1. Create a Button in your main UI (UI > Button)
2. Add onClick event to call ShopUI.ToggleShop()

## Usage

- Click Humans button to show human items
- Click Foods button to show food items
- Click Buy on items if you have enough Z-Coins
- Humans spawn at the designated spawn point
- Food adds to inventory

## Troubleshooting

- Ensure all SO assets have prefabs assigned
- Check Z-Coins balance in GameManager
- Verify InventoryManager is in scene for food purchases
- Spawn point should be positioned where you want humans to appear