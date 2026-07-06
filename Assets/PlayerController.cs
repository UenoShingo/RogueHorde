using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 移動とアニメーション
    Rigidbody2D rigidbody2d;
    Animator animator;
    float moveSpeed = 2;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        movePlayer();
    }

    // プレイヤーの移動に関する処理
    void movePlayer()
    {
        // 移動する方向
        Vector2 dir = Vector2.zero;
        // 再生するアニメーション
        string trigger = "";

        if (Input.GetKey(KeyCode.UpArrow))
        {
            dir += Vector2.up;
            trigger = "isUp";
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            dir -= Vector2.up;
            trigger = "isDown";
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            dir += Vector2.right;
            trigger = "isRight";
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            dir -= Vector2.right;
            trigger = "isLeft";
        }

        // 入力がなければ抜ける
        if (Vector2.zero == dir) return;

        // プレイヤー移動
        rigidbody2d.position += dir.normalized * moveSpeed * Time.deltaTime;

        // アニメーションを再生する
        animator.SetTrigger(trigger);
    }
}
