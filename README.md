# Paired Prototype

A 2D maze game where the player must collect colored keys, unlock doors, rescue the princess, and reach the checkpoint.

## How to Play

1. Navigate through the maze.
2. Shoot destructible colored objects. Each object is destroyed after five hits and gives you a key of the corresponding color.
3. Approach a matching colored door and press **F** to use a key and open it.
4. Approach the princess and press **F** to rescue her.
5. Fully enter the checkpoint while carrying the princess to complete the game.

## Controls

| Action                                 | Control                                                   |
| -------------------------------------- | --------------------------------------------------------- |
| Move                                   | **WASD**                                                  |
| Shoot                                  | Hold the **Attack** control configured in `Player/Attack` |
| Interact / Open door / Rescue princess | **F**                                                     |

The player automatically turns to face the current movement direction. Projectiles are fired in the direction the player is facing.

## Keys and Doors

* Red objects give red keys.
* Blue objects give blue keys.
* Green objects give green keys.
* Opening a door consumes one key of the matching color.
* Current key counts are displayed in the upper-right corner of the screen.

## Win Condition

Rescue the princess, then fully enter the checkpoint. The message **SUCCESS!** will appear when the objective is complete.
