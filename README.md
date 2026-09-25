# Paired Prototype

A 2D maze game set in a haunted castle during a storm. The princess is trapped inside: collect colored keys, unlock the doors, rescue her, and escape through the checkpoint.

## Theme

* **The storm:** dark, moonlit castle with rain and lightning (thunder follows the flash).
* **Powder kegs:** old explosive barrels. Shoot them and stay clear of the blast, or be dragged back to the gate.
* **Cursed chests:** mystery boxes that may hold a blessing (a key, speed, spirit form) or a curse (slowed down, sent back to the gate).
* **Spirit form:** press **G** to become a spirit for a few seconds. You pass through walls and are immune to powder kegs, but doors stay locked.

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
| Spirit form (walk through walls)       | **G**                                                     |

The player automatically turns to face the current movement direction. Projectiles are fired in the direction the player is facing.

## Keys and Doors

* Red objects give red keys.
* Blue objects give blue keys.
* Green objects give green keys.
* Opening a door consumes one key of the matching color.
* Current key counts are displayed in the upper-right corner of the screen.

## Win Condition

Rescue the princess, then fully enter the checkpoint. The message **You escaped the castle with the princess!** will appear when the objective is complete.
