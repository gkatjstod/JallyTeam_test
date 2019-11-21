using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class MoveAnimation : MonoBehaviour
{
    ///애니메이션 상태를 enum으로 체크
    enum ACTION_TYPE
    {
        ch_idle =0, //기본상태
        ch_leftmove, //왼쪽움직임
        ch_rightmove, //오른쪽 움직임
        ch_jump, //점프
        ch_attack, //공격
        ch_damage, //피격
        ch_dead, //뒤짐ㅎ
    }

    SkeletonAnimation SkeletonAnimation;
    public GameObject Hitbox;
    Rigidbody Myrigidbody;
    BoxCollider HitboxCollider;

    public float speed; //캐릭터 속도
    public float force; //캐릭터 점프
    public int jumpcount = 2; //2단점프
    private bool isground = true; //바닥에 닿는지 검사
    private bool lookright = true; //캐릭터의 방향


    private float PrePadH;//이전LeftStickHorizontal
    private float PrePadV;//이전LeftStickVertical
    private float Intervel;//딜레이타임

    ACTION_TYPE atype;
    bool IsPadCenter() //L스틱중립
    {
        return ((PrePadH == 0) && (Input.GetAxisRaw("LeftStickHorizontal") == 0)
                && (Input.GetAxisRaw("LeftStickVertical") == 0) && (PrePadV == 0));
    }
    bool IsPadLDown()//L스틱 좌욉력
    {
        return ((PrePadH > Input.GetAxisRaw("LeftStickHorizontal")) && (Input.GetAxisRaw("LeftStickHorizontal")<0) );
    }
    bool IsPadLUp()//L스틱 좌입력 해제
    {
        return ((PrePadH < 0) && (Input.GetAxisRaw("LeftStickHorizontal")==0) );
    }
    bool IsPadLPressing()//L스틱 좌입력중
    {
        return ((PrePadH == -1) && (Input.GetAxisRaw("LeftStickHorizontal") == -1));
    }
    bool IsPadRDown()//L스틱 우입력
    {
        return ((PrePadH < Input.GetAxisRaw("LeftStickHorizontal")) && (Input.GetAxisRaw("LeftStickHorizontal") >0));
    }
    bool IsPadRUp()//L스틱 우입력 해제
    {
        return ((PrePadH > 0) && (Input.GetAxisRaw("LeftStickHorizontal") == 0));
    }
    bool IsPadRPressing()//L스틱 우입력중
    {
        return ((PrePadH == 1) && (Input.GetAxisRaw("LeftStickHorizontal") == 1));
    }





    void Start()
    {
        SkeletonAnimation = transform.GetChild(0).GetComponent<SkeletonAnimation>();
        //Hitbox = transform.GetChild(1).GetComponent<GameObject>();
        HitboxCollider = transform.GetChild(1).GetComponent<BoxCollider>();
        Myrigidbody = GetComponent<Rigidbody>();

        SetActionType(ACTION_TYPE.ch_idle);
        PrePadH = Input.GetAxisRaw("LeftStickHorizontal");
        PrePadV = Input.GetAxisRaw("LeftStickVertical");
        Intervel = 0.0f;


    }

    ///ACTION_TYPE별 애니메이션 정의
    void SetActionType(ACTION_TYPE type)
    {
        atype = type;
        switch(atype)
        {
            case ACTION_TYPE.ch_idle:
                {
                    SkeletonAnimation.state.SetAnimation(0, "idle", true);
                }
                break;
            case ACTION_TYPE.ch_leftmove:
                {
                    SkeletonAnimation.skeleton.FlipX = true; ///좌우반전
                    SkeletonAnimation.state.SetAnimation(0, "run", true);
                }
                break;
            case ACTION_TYPE.ch_rightmove:
                {
                    SkeletonAnimation.skeleton.FlipX = false; ///좌우반전
                    SkeletonAnimation.state.SetAnimation(0, "run", true);
                }
                break;
            case ACTION_TYPE.ch_jump:
                {
                    SkeletonAnimation.state.SetAnimation(0, "jump", false);
                    Myrigidbody.AddForce(0.0f, force, 0.0f);
                }
                break;
            case ACTION_TYPE.ch_attack:
                {
                    SkeletonAnimation.state.SetAnimation(0, "attack", false);
                }
                break;
            default: break;
        }
    }

    ///실질적 액션 변경함수, 한 번만 실행
    private void InputAction()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)||
            IsPadLDown())//L스틱 좌입력
        {
            lookright = false;
            SetActionType(ACTION_TYPE.ch_leftmove);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow)||
            IsPadRDown())//L스틱 우입력
        {
            lookright = true;
            SetActionType(ACTION_TYPE.ch_rightmove);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow)||
            // Input.GetAxisRaw("LeftStickVertical") < 0 ||
            Input.GetKeyDown(KeyCode.JoystickButton1))//엑박 a번튼 / 플스 x버튼
        {
            SetActionType(ACTION_TYPE.ch_jump);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow)
            || Input.GetKeyDown(KeyCode.Joystick1Button0)
            )
          // Input.GetAxisRaw("LeftStickVertical") > 0)
        {
            if (lookright==true) HitboxCollider.center = new Vector3(1.5f, 1.5f, 0f);
            else HitboxCollider.center = new Vector3(-1.5f, 1.5f, 0f);
            Hitbox.SetActive(true);
            SetActionType(ACTION_TYPE.ch_attack);
            Invoke("MoveReset", 0.4f);
        }
        if(Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow)
            || IsPadLUp()||IsPadRUp()   //L스틱 좌우입력해제시
           //||IsPadCenter()
        )
        {
            if (atype == ACTION_TYPE.ch_jump) return;
            SetActionType(ACTION_TYPE.ch_idle);
        }
    }

    ///움직임 업데이트
    void ActionUpdate()
    {
        Vector3 pos = transform.position;
        switch (atype)
        {
            case ACTION_TYPE.ch_leftmove:
                {
                    pos.x -= Time.deltaTime * speed;
                }
                break;
            case ACTION_TYPE.ch_rightmove:
                {
                    pos.x += Time.deltaTime * speed;
                }
                break;
            case ACTION_TYPE.ch_jump:
                {
                    
                }
                break;
            default: break;
        }
        transform.position = pos;
    }

    ///공격시 히트박스 끄고 애니메이션 초기화
    void MoveReset()
    {
        Hitbox.SetActive(false);
        SetActionType(ACTION_TYPE.ch_idle);
    }

    void Update()
    {
        //PrePadH = Input.GetAxisRaw("LeftStickHorizontal");
        //PrePadV = Input.GetAxisRaw("LeftStickVertical");
        Intervel += Time.deltaTime;
        InputAction();
        ActionUpdate();
        if (Intervel > 0.1f)
        {

            PrePadH = Input.GetAxisRaw("LeftStickHorizontal");
            PrePadV = Input.GetAxisRaw("LeftStickVertical");
            Intervel = 0.0f;
        }



    }
}
