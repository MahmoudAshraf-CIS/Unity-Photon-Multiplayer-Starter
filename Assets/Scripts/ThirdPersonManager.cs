using UnityEngine;
using UnityEngine.EventSystems;

using Photon.Pun;

using System.Collections;
using Photon.Pun.Demo.PunBasics;
using Cinemachine;
using StarterAssets;

namespace Com.MyCompany.MyGame
{
    public class ThirdPersonManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region Private Fields
        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;
        #endregion


        
        public TextMesh counterTxt;
        public int counter = 0;
        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        void Awake()
        {
            counter = Random.Range(0, 1000);
            // #Important
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
            if (photonView.IsMine)
            {
                ThirdPersonManager.LocalPlayerInstance = this.gameObject;

            }
            else
            {
                // deactivate other players inputjoystick
                transform.GetChild(3).transform.gameObject.SetActive(false);
            }
            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(this.gameObject);

        }
        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        void Start()
        {
            if (photonView.IsMine)
            {
                CinemachineVirtualCamera _cameraWork = FindObjectOfType<CinemachineVirtualCamera>();


                if (_cameraWork != null)
                {
                    if (photonView.IsMine)
                    {
                        _cameraWork.Follow = this.transform.GetChild(0);
                    }
                }
                else
                {
                    Debug.LogError("<Color=Red><a>Missing</a></Color> Can not find the CinemachineVirtualCamera.", this);
                }
            }

#if UNITY_5_4_OR_NEWER
            // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
#endif
        }

#if !UNITY_5_4_OR_NEWER
/// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
void OnLevelWasLoaded(int level)
{
    this.CalledOnLevelWasLoaded(level);
}
#endif


        void CalledOnLevelWasLoaded(int level)
        {
            Debug.Log("level load " + level);
            // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
            {
                transform.position = new Vector3(0f, 5f, 0f);
            }
            if (photonView.IsMine)
            {
                CinemachineVirtualCamera _cameraWork = FindObjectOfType<CinemachineVirtualCamera>();
                if (_cameraWork != null)
                {
                    if (photonView.IsMine)
                    {
                        _cameraWork.Follow = this.transform.GetChild(0);
                    }
                }
                else
                {
                    Debug.LogError("<Color=Red><a>Missing</a></Color> Can not find the CinemachineVirtualCamera.", this);
                }
            }
             
        }
        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// </summary>
        /// 
        float elapcedTime = 0;
        void Update()
        {

            if (photonView.IsMine)
            {
                ProcessInputs();
            }

            elapcedTime+= Time.deltaTime;
            if(elapcedTime > 5)
            {
                counter++;
                counterTxt.text = counter.ToString();
                elapcedTime = 0;
            }
             
        }
#if UNITY_5_4_OR_NEWER
        public override void OnDisable()
        {
            // Always call the base to remove callbacks
            base.OnDisable();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
#endif
        #endregion

        #region Private Methods
#if UNITY_5_4_OR_NEWER
        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
        {
            this.CalledOnLevelWasLoaded(scene.buildIndex);
        }
#endif
        #endregion

        #region Custom

        /// <summary>
        /// Processes the inputs. Maintain a flag representing when the user is pressing Fire.
        /// </summary>
        void ProcessInputs()
        {
             // if my player needs an input to be processed 
        }

        #endregion
        /// <summary>
        /// MonoBehaviour method called when the Collider 'other' enters the trigger.
        /// Affect Health of the Player if the collider is a beam
        /// Note: when jumping and firing at the same, you'll find that the player's own beam intersects with itself
        /// One could move the collider further away to prevent this or check if the beam belongs to the player.
        /// </summary>
        void OnTriggerEnter(Collider other)
        {
            if (!photonView.IsMine)
            {
                return;
            }
            // We are only interested in Beamers
            // we should be using tags but for the sake of distribution, let's simply check by name.
            
        }
        /// <summary>
        /// MonoBehaviour method called once per frame for every Collider 'other' that is touching the trigger.
        /// We're going to affect health while the beams are touching the player
        /// </summary>
        /// <param name="other">Other.</param>
        void OnTriggerStay(Collider other)
        {
            // we dont' do anything if we are not the local player.
            if (!photonView.IsMine)
            {
                return;
            }
        }
        #region IPunObservable implementation
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(counter);
            }
            else
            {
                // Network player, receive data
                this.counter = (int)stream.ReceiveNext();
                counterTxt.text = counter.ToString();
            }
        }
        #endregion

    }
}