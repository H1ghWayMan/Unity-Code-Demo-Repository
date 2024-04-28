using Sirenix.OdinInspector;
using UnityEngine;

namespace Kaiju
{
    /// <summary>
    /// Base class for enemies in the game world.
    /// </summary>
    public class Enemy : Entity
    {
        //states
        [HideInInspector] public ChaseState chase;
        [HideInInspector] public AttackState attack;
        [HideInInspector] public RetreatState retreat;
        [HideInInspector] public PulledState pulled;
        [HideInInspector] public GrabbedState grabbed;
        [HideInInspector] public ThrownState thrown;
        [HideInInspector] public PulledDestroyedState pulledDestroyed;
        [HideInInspector] public GrabbedDestroyedState grabbedDestroyed;
        [HideInInspector] public ThrownDestroyedState thrownDestroyed;
        [HideInInspector] public CircleState circle;
        [HideInInspector] public ResupplyState resupply;

        // Properties
        [HideInInspector] public Vector2 playerTriggerDistance;
        [HideInInspector] public float retreatDistance;
        [HideInInspector] public EnemyCategory category { get; protected set; }

        // Providers
        public MovementProvider movementProvider { get; protected set; }
        public AttackProvider attackProvider { get; protected set; }
        public DropProvider dropProvider { get; protected set; }

        // References
        public Transform myEnemy; // Reference to the enemy's transform
        public Rigidbody myRB; // Reference to the enemy's rigidbody
        public bool wasDestroyed = false; // Flag indicating if the enemy was destroyed

        // Initialization status
        public bool Inited { get; protected set; }

        // Destruction settings
        [FoldoutGroup("Destruction")]
        [TitleGroup("Destruction/Audio")]
        public AudioClip DestroySound; // Sound played upon destruction
        [TitleGroup("Destruction/Models")]
        public GameObject WreckModel; // Model used when the enemy is destroyed
        [TitleGroup("Destruction/Models")]
        public GameObject StandardModel; // Standard model of the enemy
        [TitleGroup("Destruction/FX")]
        public GameObject ExplosionObj; // Explosion effect upon destruction

        public EnemyPreset enemyPreset; // Preset for the enemy

        // Button to destroy the enemy
        [Button("Destroy"), DisableIf("@entitySM == null || entitySM.CurrentState == null || entitySM.CurrentState is DestroyedState")]
        public void DestroyEnemy() => entitySM.ChangeState(destroy);

        private void Awake()
        {
            if (enemyPreset != null && !Inited) Init(enemyPreset); // Initialize if preset is available and not already initializ
        }

        /// <summary>
        /// Initializes the enemy with the specified preset.
        /// </summary>
        /// <param name="preset">The preset to initialize with.</param>
        public virtual void Init(EnemyPreset preset)
        {
            statesInited = InitStates(); // Initialize states

            // Initialize providers
            movementProvider = GetComponent<MovementProvider>();
            attackProvider = GetComponent<AttackProvider>();
            dropProvider = GetComponent<DropProvider>();
            myRB = GetComponent<Rigidbody>();

            category = preset.category; // Set the category of the enemy based on the preset

            // Initialize movement, attack, and drop providers if available
            movementProvider?.Init(this, preset);
            attackProvider?.Init(this, preset);
            dropProvider?.Init(this);

            playerTriggerDistance = preset.playerTriggerDistance; // Set player trigger distance from preset

            retreatDistance = preset.retreatDistance; // Set retreat distance from preset

            // Initialize used HP controller if not already set
            if (usedHpController == null)
                usedHpController = GetComponent<HPController>();

            Inited = true;// Set the initialization flag to true
        }

        /// <summary>
        /// Initializes the states for the enemy.
        /// </summary>
        /// <returns>True if initialization succeeds, false otherwise.</returns>
        public virtual bool InitStates()
        {
            // This method can be overridden by derived classes to initialize specific states.
            // By default, it returns true indicating successful initialization.
            return true;
        }

        /// <summary>
        /// Counts the death of the enemy and triggers related events.
        /// </summary>
        /// <param name="damageType">The type of damage that caused the death.</param>
        public void CountDeath(DamageReceiver.DamageType damageType)
        // Check if the event dispatcher for unit fallen event is not null
        if (ChallengeEnemyDestruction.OnUnitFallenEvDispatcher != null)
        {
            // Trigger the unit fallen event dispatcher with the enemy category and damage type
            ChallengeEnemyDestruction.OnUnitFallenEvDispatcher(category, damageType);
        }
} 
}
