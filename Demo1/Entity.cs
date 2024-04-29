using Sirenix.OdinInspector;
using UnityEngine;

namespace Kaiju
{
    /// <summary>
    /// Base class for entities in the game world.
    /// </summary>
    public class Entity : MonoBehaviour
    {
        // Fields for different states
        [HideInInspector] public StateMachine entitySM;
        [HideInInspector] public InitializeState initialize;
        [HideInInspector] public ErrorState error;
        [HideInInspector] public DestroyedState destroy;
        [HideInInspector] public SpawnState spawn;
        [HideInInspector] public DespawnState despawn;

        // Protected fields
        protected bool errorBool = false; // Flag indicating if there's an error
        protected bool statesInited; // Flag indicating if states are initialized
        protected AudioClipInvoker audioClipInvoker; // Used for playing audio clips
        [ReadOnly]
        public string currentState; // Current state of the entity


        // Components
        public HPController usedHpController; // Health controller


        public virtual void Update()
        {
            if (!statesInited)
                return;

            entitySM.CurrentState.Tick(); // Execute the tick function of the current state
            currentState = entitySM.CurrentState.Name; // Update the current state
        }

        public virtual void FixedUpdate()
        {
            if (!statesInited)
                return;

            entitySM.CurrentState.PhysicsTick(); // Execute physics tick function of the current state
        }

        /// <summary>
        /// Plays a sound with the given ID.
        /// </summary>
        /// <param name="id">ID of the sound to play.</param>
        public void PlaySound(string id)
        {
            audioClipInvoker = GetComponent<AudioClipInvoker>();
            if (audioClipInvoker != null)
            {
                audioClipInvoker.Play(id); // Play the sound
            }
        }
        /// <summary>
        /// Sets the entity state to "Destroyed".
        /// </summary>
        public void SetStateDestroyed()
        {
            entitySM.ChangeState(destroy); // Change the state to "Destroyed"
        }
        /// <summary>
        /// Destroys the entity after a specified delay.
        /// </summary>
        public void DestroyMe(float delay)
        {
            Destroy(gameObject, delay); // Destroy the entity after a delay
        }
    } 
}
