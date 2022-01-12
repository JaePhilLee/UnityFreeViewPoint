using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FreeViewPoint : MonoBehaviour
{
    public float moveSpeed = 0.01f;

    public float yMaxRotateLimit = 45.0f;
    public float yMinRotateLimit = -45.0f;

    float mobileManipulation = 50.0f; //회전 시 모바일은 PC보다 기민하므로 보정하기 위한 값
    float yRotCounter = 0.0f;
    float xRotCounter = 0.0f;

    bool tabbed = false;
    float maxTimeDoubleTab = 0.5f; //0.5초

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        #if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
            // Mobile( Android || iOS )
            MobileProcess();
        #else
            // Other Platforms
            DefaultProcess();
        #endif
    }

    void DefaultProcess() {
        // Zoom In/Out : 마우스 휠
        Zoom( Input.GetAxis("Mouse ScrollWheel") );

        // Rotate : 마우스 좌 클릭 후 드래그
        if ( Input.GetMouseButton( 0 ) && !EventSystem.current.IsPointerOverGameObject() ) {
            Rotate(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }

        // Move : W,A,S,D 키 입력
        Vector3 moveVector = Vector3.zero;
        // 좌/우키 동시 눌렀을 때, A(좌)키 우선 순위
        if ( Input.GetKey(KeyCode.A) ) {
            moveVector += new Vector3(-moveSpeed, 0.0f, 0.0f);
        } else if ( Input.GetKey(KeyCode.D) ) {
            moveVector += new Vector3(moveSpeed, 0.0f, 0.0f);
        }

        // 앞/뒤키 동시 눌렀을 때, W(앞)키 우선 순위
        if ( Input.GetKey(KeyCode.W) ) {
            moveVector += new Vector3(0.0f, 0.0f, moveSpeed);
        } else if ( Input.GetKey(KeyCode.S) ) {
            moveVector += new Vector3(0.0f, 0.0f, -moveSpeed);
        }

        // 카메라가 바라보는 방향 기준 이동
        if ( moveVector != Vector3.zero ) {
            Move( moveVector );
        }
    }

    void MobileProcess() {
        //1개의 터치라도 UI Touch일 경우 실행 X
        for ( int i=0;i<Input.touchCount;i++ ) {
            if ( EventSystem.current.IsPointerOverGameObject(Input.touches[i].fingerId) ) {
                return;
            }
        }

        
        switch ( Input.touchCount ) {
            case 1:
                // Rotate : 1개의 손가락으로 터치 후 드래그
                Rotate( Input.touches[0].deltaPosition.x / mobileManipulation, Input.touches[0].deltaPosition.y / mobileManipulation );
            break;
            case 2:
                // Zoom In/Out : 2개의 손가락을 이용해 확대/축소
                Touch firstTouch = Input.touches[0];
                Touch secondTouch = Input.touches[1];

                //deltaPosition은 delta만큼 시간동안 움직인 거리.
                //현재 Poisition에서 빼주면, 움직이기 이전의 위치.
                Vector2 firstPrevPos = firstTouch.position - firstTouch.deltaPosition;
                Vector2 secondPrevPos = secondTouch.position - secondTouch.deltaPosition;
                
                //과거와 현재의 움직인 길이를 구함.
                float prevTouchMag = (firstPrevPos - secondPrevPos).magnitude;
                float currentTouchMag = (firstTouch.position - secondTouch.position).magnitude;

                //Zoom In/Out시 얼만큼 많이 In/Out이 될지에 대한 차이
                float diff = prevTouchMag - currentTouchMag;
                Debug.Log(" Zooming : " + diff);

                Zoom(diff * -0.01f);
            break;
            case 3:
                // Move : 3개의 손가락 중 두 번째 손가락을 기준으로 이동
                Move( new Vector3(Input.touches[1].deltaPosition.x / mobileManipulation, Input.touches[1].deltaPosition.y / mobileManipulation, 0.0f) );
            break;
        }
    }

    // bool IsDoubleTap(){
    //     bool result = false;
    //     float MaxTimeWait = 1;
    //     float VariancePosition = 1;

    //     if( Input.touchCount == 1  && Input.GetTouch(0).phase == TouchPhase.Began)
    //     {
    //         float DeltaTime = Input.GetTouch (0).deltaTime;
    //         float DeltaPositionLenght=Input.GetTouch (0).deltaPosition.magnitude;

    //         if ( DeltaTime> 0 && DeltaTime < MaxTimeWait && DeltaPositionLenght < VariancePosition)
    //             result = true;                
    //     }

    //     return result;
    //  }

    void Rotate(float inputX, float inputY) {
        //카메라 회전 (위/아래, 좌/우)
        // transform.Rotate(new Vector3(inputY * rotateSpeed, -1 * inputX * rotateSpeed, 0));
        // transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);

        xRotCounter += inputX * 1000.0f * Time.deltaTime;
        yRotCounter += inputY * 1000.0f * Time.deltaTime;
        yRotCounter = Mathf.Clamp(yRotCounter, yMinRotateLimit, yMaxRotateLimit);
        transform.localEulerAngles = new Vector3(-yRotCounter, xRotCounter, 0);
    }

    void Zoom(float zoom) {
        transform.Translate(Vector3.forward * zoom);
    }

    void Move(Vector3 moveVector) {
        transform.position += transform.TransformDirection( moveVector );
    }
}
