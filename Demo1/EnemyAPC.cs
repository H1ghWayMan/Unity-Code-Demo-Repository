using UnityEditor;
using UnityEngine;

namespace Kaiju
{
    /// <summary>
    /// Represents an enemy APC (Armored Personnel Carrier).
    /// </summary>
    public class EnemyAPC : Enemy
    {
        [SerializeField] float destinationBuffer = 0.5f; // Buffer distance for destination

        [Header("Debug")]
        [SerializeField] bool drawRangeGizmos; // Whether to draw range gizmos for debugging


        /// <summary>
        /// Initializes the enemy APC with the specified preset.
        /// </summary>
        /// <param name="preset">The preset to initialize with.</param>
        public override void Init(EnemyPreset preset)
        {
            base.Init(preset);

            if (!statesInited)
                return;

            entitySM.ChangeState(spawn);
        }

        /// <summary>
        /// Initializes the state machine and states for the enemy APC.
        /// </summary>
        /// <returns>True if initialization succeeds, false otherwise.</returns>
        public override bool InitStates()
        {
            entitySM = new StateMachine(); // Create a new state machine

            // Initialize states
            initialize = new InitializeState(this, entitySM);
            chase = new APCChase(this, entitySM, destinationBuffer);
            attack = new APCAttack(this, entitySM);
            retreat = new APCRetreat(this, entitySM);
            destroy = new APCDestroyed(this, entitySM);
            error = new ErrorState(this, entitySM);
            spawn = new APCSpawn(this, entitySM);
            pulled = new APCPulled(this, entitySM);
            grabbed = new APCGrabbed(this, entitySM);
            thrown = new APCThrown(this, entitySM);
            pulledDestroyed = new APCPulledDestroyed(this, entitySM);
            grabbedDestroyed = new APCGrabbedDestroyed(this, entitySM);
            thrownDestroyed = new APCThrownDestroyed(this, entitySM);
            despawn = new APCDespawn(this, entitySM);

            if (errorBool)
            {
                entitySM.Initialize(error); // Initialize with error state if an error occurred
                return false;
            }

            entitySM.Initialize(initialize); // Initialize with initialize state

            return true;
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!drawRangeGizmos || myEnemy == null)
                return;

            var center = new Vector3(myEnemy.position.x, transform.position.y, myEnemy.position.z);

            Handles.color = Color.red;
            Handles.DrawWireArc(center, Vector3.up, Vector3.right, 360, playerTriggerDistance.x, 3);
            Handles.color = Color.green;
            Handles.DrawWireArc(center, Vector3.up, Vector3.right, 360, playerTriggerDistance.y, 3);
            Handles.color = Color.yellow;
            Handles.DrawWireArc(center, Vector3.up, Vector3.right, 360, playerTriggerDistance.x + destinationBuffer, 1);
            Handles.DrawWireArc(center, Vector3.up, Vector3.right, 360, playerTriggerDistance.y - destinationBuffer, 1);

            var agent = (movementProvider as NavMeshMovementProvider).agent;
            if (agent)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(agent.destination, 0.2f);
            }
        }
#endif
    } 
}
