using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DoorScript
{
    [RequireComponent(typeof(AudioSource))]
    public class Door : MonoBehaviour
    {
        public float smooth = 1.0f;
        public float interactDistance = 2.5f;

        [Header("Puertas adicionales")]
        public Door[] extraDoors;

        float DoorOpenAngle = -90.0f;

        public AudioSource asource;
        public AudioClip openDoor;

        private bool open = false;
        private Transform _player;

        void Start()
        {
            asource = GetComponent<AudioSource>();

            var controller = FindObjectOfType<StarterAssets.ThirdPersonController>();
            if (controller != null)
                _player = controller.transform;
        }

        void Update()
        {
            if (open)
            {
                var target = Quaternion.Euler(0, DoorOpenAngle, 0);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, target, Time.deltaTime * 5 * smooth);
            }

            if (_player == null) return;

            float dist = Vector3.Distance(transform.position, _player.position);
            if (!open && dist <= interactDistance && Input.GetKeyDown(KeyCode.E))
                OpenDoor();
        }

        public void OpenDoor()
        {
            if (open) return;

            open = true;
            asource.clip = openDoor;
            asource.Play();

            foreach (Door door in extraDoors)
            {
                if (door != null)
                    door.OpenDoor();
            }
        }
    }
}