using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class PlayerCtrl : MonoBehaviour
{
    //컴포넌트 캐시 처리할 변수
    private Transform tr;

    //Animation 컴포넌트를 저장할 변수
    private Animation anim;

    //이동 속도 변수 (public으로 선언되어 인스펙터 뷰에 노출됨)
    public float moveSpeed = 10.0f;

    //회전 속도 변수
    public float turnSpeed = 80.0f;

    //초기 생명 값
    private readonly float initHp = 100.0f;
    //현재 생명 값
    public float currHp;

    //Hpbar 연결할 변수
    private Image hpBar;

    //델리게이트 선언
    public delegate void PlayerDieHandler();

    //이벤트 선언
    public static event PlayerDieHandler OnPlayerDie;

    void Awake()
    {
        //제일 먼저 호출되는 함수
        //스크립트가 비활성화돼 있어도 호출되는 함수
        
    }

    void OnEnable()
    {
        //두 번째로 호출되는 함수
        //스크립트가 활성화될 때마다 호출되는 함수

    }

    IEnumerator Start()
    {
        //Hpbar 연결
        hpBar = GameObject.FindGameObjectWithTag("HP_BAR")?.GetComponent<Image>();
        //HP 초기화
        currHp = initHp;

        //세 번째로 호출되는 함수
        //Update 함수가 호출되기 전에 호출되는 함수
        //코루틴(Coroutine)으로 호출될 수 있는 함수 (예 : IEnumerator Start() {})
        tr = GetComponent<Transform>();
                //함수명   형식(추출할 클래스)    <>() <-인자 매개변수
                //tr = this.gameObject.GetComponent<Transform>();
                //"이 스크립트가 포함된 게임오브젝트가 가진 여러 컴포넌트 중에서 Transform 컴포넌트를 추출해 tr 변수에 저장하라."
        anim = GetComponent<Animation>();

        //애니메이션 실행
        anim.Play("Idle");

        turnSpeed = 0.0f;
        yield return new WaitForSeconds(0.3f);
        turnSpeed = 80.0f;
    }

    void Update()
    {
        //프레임마다 호출되는 함수
        //호출 간격이 불규칙적인 함수
        //화면의 렌더링 주기와 일치
        float h = Input.GetAxis("Horizontal"); // -1.0f ~ 0.0f ~ +1.0f
        float v = Input.GetAxis("Vertical"); // -1.0f ~ 0.0f ~ +1.0f
        float r = Input.GetAxis("Mouse X");

        //전후좌우 이동 방향 벡터 계산
        Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);

        //Translate(이동방향 * 속력 * Time.deltaTime)
        tr.Translate(moveDir.normalized * moveSpeed * Time.deltaTime);

        //Vector3.up 축을 기준으로 turnSpeed만큼의 속도로 회전
        tr.Rotate(Vector3.up * turnSpeed * Time.deltaTime * r);

        //Transform 컴포턴트의 position 속성값을 변경
        //transform.position += new Vector3(0, 0, 1);

        //정규화 벡터를 사용한 코드
        //transform.position += Vector3.forward * 1;

        //Translate 함수를 사용한 이동 로직
        //tr.Translate({이동할방향} * Time.deltaTime * {전진/후진 변수} * {속도});
        //tr.Translate(Vector3.forward * Time.deltaTime * v * moveSpeed);

        //주인공 캐릭터의 애니메이션 설정
        PlayerAnim(h, v);
    }

    void PlayerAnim(float h, float v)
    {
        //키보드 입력값을 기준을 ㅗ동작할 애니메이션 수행
        if(v >= 0.1f){
            anim.CrossFade("RunF", 0.25f); // 전진 애니메이션 실행
        }else if (v <= -0.1f)
        {
            anim.CrossFade("RunB", 0.25f); // 후진 애니메이션 실행
        }else if (h >= 0.1f){
            anim.CrossFade("RunR", 0.25f); // 오른쪽 이동 애니메이션 실행
        }else if (h <= -0.1f){
            anim.CrossFade("RunL", 0.25f); // 왼쪽 이동 애니메이션 실행
        }else{
            anim.CrossFade("Idle", 0.25f); // 정지 시 Idle 애니메이션 실행
        }
    }

    //충돌한 Collider의 IsTrigger 옵션이 체크됐을 때 발생
    void OnTriggerEnter(Collider coll)
    {
        //충돌한 Collider가 몬스터의 PUNCH이면 Player HP 차감
        if(currHp >= 0.0f && coll.CompareTag("PUNCH")){
            currHp -=10.0f;
            DisplayHealth();

            Debug.Log($"Player hp = {currHp/initHp}");

            //Player의 생명이 0이하이면 사망처리
            if(currHp <= 0.0f){
                PlayerDie();
            }
        }
    }

    //Player의 사망 처리
    void PlayerDie()
    {
        Debug.Log("Player Die !");

        // //MONSTER 태그를 가진 모든 게임오브젝트를 찾아옴
        // GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");

        // //모든 몬스터의 OnPlayerDie 함수를 순차적으로 호출
        // foreach(GameObject monster in monsters){
        //     monster.SendMessage("OnPlayerDie", SendMessageOptions.DontRequireReceiver);
        // }

        //주인공 사망 이벤트 호출(발생)
        OnPlayerDie();

        //GameManager 스크립트의 IsGameOver 프로퍼티 값을변경
        //GameObject.Find("GameMgr").GetComponent<GameManager>().IsGameOver = true;
        GameManager.instance.IsGameOver = true;
    }

    void DisplayHealth()
    {
        hpBar.fillAmount = currHp/initHp;
    }

    void LateUpdate()
    {
        //Update 함수가 종료된 후 호출되는 함수
    }

    void FixedUpdate()
    {
        //일정한 간격으로 호출되는 함수 (기본값 0.02초)
        //물리 엔진의 계산 주기와 일치
    }
}
