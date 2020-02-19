//
//SpringBone.cs for unity-chan!
//
//Original Script is here:
//ricopin / SpringBone.cs
//Rocket Jump : http://rocketjump.skr.jp/unity3d/109/
//https://twitter.com/ricopin416
//
//Revised by N.Kobayashi 2014/06/20
//

using UnityEngine;
using System.Collections;
using System.Threading;
using System.Collections.Generic;

namespace UnityChan {
    public class SpringBone : MonoBehaviour {
        //次のボーン
        public Transform child;

        //ボーンの向き
        public Vector3 boneAxis = new Vector3(-1.0f, 0.0f, 0.0f);
        public float radius = 0.05f;

        //各SpringBoneに設定されているstiffnessForceとdragForceを使用するか？
        public bool isUseEachBoneForceSettings = false;

        //バネが戻る力
        public float stiffnessForce = 0.01f;

        //力の減衰力
        public float dragForce = 0.4f;
        public Vector3 springForce = new Vector3(0.0f, -0.0001f, 0.0f);
        public SpringCollider[] colliders;
        public bool debug = true;
        //Kobayashi:Thredshold Starting to activate activeRatio
        public float threshold = 0.01f;
        private float springLength;
        private Quaternion localRotation;
        private Transform trs;
        private Vector3 currTipPos;
        private Vector3 prevTipPos;
        //Kobayashi
        //private Transform org;
        //Kobayashi:Reference for "SpringManager" component with unitychan 
        private SpringManager managerRef;

        private void Awake() {
            trs = transform;
            localRotation = transform.localRotation;
            //Kobayashi:Reference for "SpringManager" component with unitychan
            // GameObject.Find("unitychan_dynamic").GetComponent<SpringManager>();
            managerRef = GetParentSpringManager(transform);
        }

        private SpringManager GetParentSpringManager(Transform t) {
            var springManager = t.GetComponent<SpringManager>();

            if (springManager != null)
                return springManager;

            if (t.parent != null) {
                return GetParentSpringManager(t.parent);
            }

            return null;
        }

        private void Start() {
            var list = GetComponentsInChildren<SpringBone>();
            if (list.Length > 1) {
                child = list[1].gameObject.transform;
            } else {
                child = transform;
            }

            springLength = Vector3.Distance(trs.position, child.position);
            currTipPos = child.position;
            prevTipPos = child.position;

            for (int i = 0; i < colliders.Length; i++) {
                if (colliders[i] == null)
                    Debug.LogError("SpringBone collider为null go=" + transform.name);

            }

            lastRotaion = trs.rotation;
        }

        Queue<Quaternion> bufList = new Queue<Quaternion>();

        public void Cal() {
            //先拿localEuler
            //设置localRotation 获取 rotation 和 position 和transformfromdirection
            //设置managerRef
            CalEnd();
            Vector3 localEuler = trs.localEulerAngles;
            trs.localRotation = Quaternion.identity * localRotation;
            var rotation = trs.rotation;
            var position = trs.position;
            var direction = trs.TransformDirection(boneAxis);
            if (managerRef == null) {
                managerRef = GetParentSpringManager(trs);
            }
            NewUpdateSpring(localEuler, rotation, position, direction);
        }

        public void CalEnd() {
            if (bufList.Count > 0) {
                trs.rotation = bufList.Dequeue();
            }
        }

        public void NewUpdateSpring(Vector3 localEulerAngles, Quaternion rotation, Vector3 position, Vector3 aimVector) {
            float sqrDt = Time.deltaTime * Time.deltaTime;
            Vector3 force = rotation * (boneAxis * stiffnessForce) / sqrDt;
            force += (prevTipPos - currTipPos) * dragForce / sqrDt;
            force += springForce / sqrDt;
            Vector3 temp = currTipPos;
            currTipPos = (currTipPos - prevTipPos) + currTipPos + (force * sqrDt);
            currTipPos = ((currTipPos - position).normalized * springLength) + position;
            for (int i = 0; i < colliders.Length; i++) {
                if (colliders[i] == null)
                    continue;
                Vector3 collisionCenterPos;
                if (colliders[i].CheckCollision(currTipPos, radius, out collisionCenterPos)) {
                    Vector3 normal = (currTipPos - collisionCenterPos).normalized;
                    currTipPos = collisionCenterPos + (normal * (radius + colliders[i].radius));
                    currTipPos = ((currTipPos - position).normalized * springLength) + position;
                }
            }
            prevTipPos = temp;
            Quaternion aimRotation = Quaternion.FromToRotation(aimVector, currTipPos - position);
            Quaternion secondaryRotation = aimRotation * rotation;
            bufList.Enqueue(Quaternion.Lerp(rotation, secondaryRotation, managerRef.dynamicRatio));
        }

        public void UpdateSpring() {

            Vector3 localEulerAngles = trs.localEulerAngles;
            //UnityEngine.Debug.Log("计算前:" + localEulerAngles.Json());

            //Kobayashi
            //org = trs;
            //回転をリセット
            trs.localRotation = Quaternion.identity * localRotation;

            float sqrDt = Time.deltaTime * Time.deltaTime;

            //stiffness
            Vector3 force = trs.rotation * (boneAxis * stiffnessForce) / sqrDt;

            //drag
            force += (prevTipPos - currTipPos) * dragForce / sqrDt;

            force += springForce / sqrDt;

            //前フレームと値が同じにならないように
            Vector3 temp = currTipPos;

            //verlet
            currTipPos = (currTipPos - prevTipPos) + currTipPos + (force * sqrDt);

            //長さを元に戻す
            currTipPos = ((currTipPos - trs.position).normalized * springLength) + trs.position;

            //衝突判定
            for (int i = 0; i < colliders.Length; i++) {
                if (colliders[i] == null)
                    continue;

                Vector3 collisionCenterPos;
                if (colliders[i].CheckCollision(currTipPos, radius, out collisionCenterPos)) {
                    Vector3 normal = (currTipPos - collisionCenterPos).normalized;
                    currTipPos = collisionCenterPos + (normal * (radius + colliders[i].radius));
                    currTipPos = ((currTipPos - trs.position).normalized * springLength) + trs.position;
                }

            }

            prevTipPos = temp;

            //回転を適用；
            Vector3 aimVector = trs.TransformDirection(boneAxis);

            Quaternion aimRotation = Quaternion.FromToRotation(aimVector, currTipPos - trs.position);
            //original
            //trs.rotation = aimRotation * trs.rotation;
            //Kobayahsi:Lerp with mixWeight
            Quaternion secondaryRotation = aimRotation * trs.rotation;

            if (managerRef == null) {
                managerRef = GetParentSpringManager(trs);
            }
            trs.rotation = Quaternion.Lerp(trs.rotation, secondaryRotation, managerRef.dynamicRatio);
            //if (isRang) {
            //    Quaternion old = trs.rotation;
            //    trs.rotation = secondaryRotation;
            //    Vector3 oldV = trs.localEulerAngles;
            //    if (oldV != localEulerAngles) {
            //        UnityEngine.Debug.Log("确实不同");
            //    } else {
            //        UnityEngine.Debug.Log("其实相同");
            //    }
            //    Check(ref oldV.x, localEulerAngles.x, rangAngle.x);
            //    Check(ref oldV.y, localEulerAngles.y, rangAngle.y);
            //    Check(ref oldV.z, localEulerAngles.z, rangAngle.z);

            //    trs.localEulerAngles = oldV;
            //    secondaryRotation = trs.rotation;

            //    trs.rotation = lastRotaion = Quaternion.Lerp(lastRotaion, secondaryRotation, managerRef.dynamicRatio);
            //} else {
            //    trs.rotation = Quaternion.Lerp(trs.rotation, secondaryRotation, managerRef.dynamicRatio);
            //}
        }

        Quaternion lastRotaion;

        static void Check(ref float dst, float src, float rang) {
            float angle = Mathf.DeltaAngle(dst, src);
            if (Mathf.Abs(angle) > rang) // 超出范围了
                dst = src + rang * (angle > 0 ? -1 : 1);
        }

        [SerializeField]
        bool isRang = false;

        [SerializeField]
        Vector3 rangAngle;

        private void OnDrawGizmos() {
            if (debug) {
                Gizmos.color = Color.yellow;
                if (Application.isPlaying)
                    Gizmos.DrawWireSphere(currTipPos, radius);
                else {
                    if (child != null)
                        Gizmos.DrawWireSphere(child.position, radius);
                }
            }
        }
    }
}
