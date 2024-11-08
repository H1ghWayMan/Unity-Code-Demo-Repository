
# Unity Code Demo Repository

This GitHub repository showcases examples of Unity scripts developed by me as a demonstration of my programming proficiency and expertise in game development. The repository includes implementations of two distinct projects: Kaiju Monster VR and RPG Helper, serving as illustrations of my capabilities in creating diverse and engaging gameplay experiences.

###### This repository is intended to demonstrate my coding abilities and progress and should be viewed as a portfolio. Contributions, feedback, and inquiries are welcome, and interested parties are encouraged to reach out for collaboration opportunities or further discussions here or on LinkedIn: https://www.linkedin.com/in/bartosz-kempisty/

## Demo1 - Enemy Implementation in Kaiju Monster VR (2022)

This collection of scripts forms the backbone of enemy implementation within the Kaiju Monster VR game, available on various platforms. Designed to define the behavior, interactions, and specialized functionalities of enemies encountered by players, these scripts are instrumental in creating immersive and challenging gameplay experiences.

![](https://cdn.altlabvr.com/11207.jpegu97w1687454456.jpeg?quality=80&type=jpg&width=1568)


#### Enemy.cs

This script is part of the Kaiju Monster VR game, available on Steam, Oculus Store, and SideQuest. It defines the behavior of enemies within the game, including various states such as Chase, Attack, Retreat, and Destroyed. Enemies are initialized with preset configurations and are capable of movement, attacking, and dropping items. The script also handles enemy destruction, including visual and audio effects, as well as triggering events upon enemy demise.

 - Implements enemy behavior, including movement, attacking, and dropping items.
 - Manages enemy states such as Chase, Attack, Retreat, and Destroyed.
 - Handles enemy destruction with visual and audio effects.
 - Triggers events upon enemy demise for game progression and scoring.
 - This script is an integral part of the Kaiju Monster VR game, defining the behavior of enemies encountered by players.
 - It is used to instantiate and control various enemy types, ensuring dynamic and engaging gameplay experiences.


#### EnemyAPC.cs

EnemyAPC is a specialized enemy script representing Armored Personnel Carriers (APCs) in the Kaiju Monster VR game. It inherits functionality from the Enemy class and adds specific behaviors tailored to APCs, such as chasing players within a certain range, attacking targets, and retreating when damaged. The script includes state machine logic for managing APC states and utilizes Gizmos for debugging and visualization of range parameters.

 - Specialized implementation for Armored Personnel Carriers (APCs) within the game.
 - Inherits functionality from the Enemy class and adds behaviors specific to APCs.
 - Manages APC states such as Chase, Attack, Retreat, and Destroyed using a state machine.
 - Utilizes Gizmos for debugging and visualization of range parameters during development.


#### Entity.cs

Entity serves as the base class for all entities in the Kaiju Monster VR game, including enemies and environmental objects. It encapsulates common functionality such as state machine management, error handling, and sound playback. The script facilitates entity updates and physics ticks, along with methods for triggering destruction and playing audio clips. Entity.cs forms the foundation for implementing diverse entities within the game, ensuring consistent behavior and functionality across various elements.

 - Base class for all entities in the Kaiju Monster VR game, providing common functionality and behaviors.
 - Manages state machine logic, error handling, and sound playback for entities.
 - Facilitates entity updates and physics ticks, ensuring smooth interaction within the game world.
 - The script streamlines entity development and ensures consistency in behavior and functionality across various elements within the game.



## Demo2 - Initiative Manager in RPG Helper (2023)

This Unity script is part of a small project aimed at enhancing tabletop RPG sessions by providing a streamlined method for managing initiative order, dice rolls, and turn-based actions. The project was designed to facilitate communication and gameplay flow among players and the Game Master (GM), without disrupting the immersion of the RPG experience.

### Features:

 - *Initiative Management:* Handles the order in which players and NPCs take their turns during combat encounters.
 - *Roll Tracking:* Tracks dice rolls made by players and NPCs, allowing the GM to request specific rolls and update the initiative order accordingly.
 - *Turn-Based Actions:* Enables players to take their turns seamlessly, with visual indicators and UI feedback to keep everyone informed.
 - *Drag-and-Drop Interaction:* Supports touch interaction for intuitive drag-and-drop functionality, enabling easy reordering of initiative order.

### Usage

 - This script is intended to run on the host device of the RPG session, serving as the central hub for initiative management.
 - Actions such as removing a roll or disposing of the initiative order can be initiated through button clicks or touch interactions with panels.

##### Note:
This project focuses solely on UI elements and does not include additional visual assets or gameplay mechanics. It is designed to integrate seamlessly into tabletop RPG sessions, enhancing the gameplay experience without disrupting the immersion of the narrative.



## Demo3 - Player Controller for *3D Game Prototype* (2024)

This *PlayerController* script is part of a demo showcasing character control mechanics, event handling, and dependency injection through separate handler classes. The code provides player movement, camera control, and event tracking.

**PlayerController.cs**  
The script relies on dependency injection to keep player movement, shooting, and input handling modular, each managed by distinct handler classes injected at runtime. These handlers allow for easier customization and testing of individual player mechanics.

### Features:

 - *Movement, Shooting, and Input Handlers:* Designed to abstract and organize input and actions, each handler can be modified or replaced without altering the core PlayerController logic.
 - *Events:* Uses custom events to trigger and manage power-ups and weapon cooldowns, keeping the controller script focused on state changes rather than event logic.
 - *Movement State Management:* Implements simple state machine specifically for handling movement actions like *Moving* or *Stationary*.
