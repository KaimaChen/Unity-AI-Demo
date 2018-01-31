using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 规则系统(RBS rule-based system)
/// 玩家出两招，然后计算机预测第三招
/// 参考《游戏开发中的人工智能》
/// </summary>
public class StrikesDemo : MonoBehaviour {
    public Text labelPrediction;
    public Text labelRandomPrediction;
    public Text labelCount;
    public Text labelSuccess;
    public Text labelRandomSuccess;

    struct WorkingMemory
    {
        public StrikesType strikeA; //上一招
        public StrikesType strikeB; //当前招
        public StrikesType strikeC; //预测招
    }

    const int NUM_RULES = 27;

    WorkingMemory mWorkingMemory = new WorkingMemory();
    StrikesRule[] mRules = new StrikesRule[NUM_RULES];
    int mCount;
    int mSuccessNum;
    int mRandomSuccessNum;
    int mPreviousRuleIndex;
    StrikesType mRandomPrediction;

	void Start () {
        mRules[0] = new StrikesRule(StrikesType.Punch, StrikesType.Punch, StrikesType.Punch);
        mRules[1] = new StrikesRule(StrikesType.Punch, StrikesType.Punch, StrikesType.LowKick);
        mRules[2] = new StrikesRule(StrikesType.Punch, StrikesType.Punch, StrikesType.HighKick);
        mRules[3] = new StrikesRule(StrikesType.Punch, StrikesType.LowKick, StrikesType.Punch);
        mRules[4] = new StrikesRule(StrikesType.Punch, StrikesType.LowKick, StrikesType.LowKick);
        mRules[5] = new StrikesRule(StrikesType.Punch, StrikesType.LowKick, StrikesType.HighKick);
        mRules[6] = new StrikesRule(StrikesType.Punch, StrikesType.HighKick, StrikesType.Punch);
        mRules[7] = new StrikesRule(StrikesType.Punch, StrikesType.HighKick, StrikesType.LowKick);
        mRules[8] = new StrikesRule(StrikesType.Punch, StrikesType.HighKick, StrikesType.HighKick);
        mRules[9] = new StrikesRule(StrikesType.LowKick, StrikesType.Punch, StrikesType.Punch);
        mRules[10] = new StrikesRule(StrikesType.LowKick, StrikesType.Punch, StrikesType.LowKick);
        mRules[11] = new StrikesRule(StrikesType.LowKick, StrikesType.Punch, StrikesType.HighKick);
        mRules[12] = new StrikesRule(StrikesType.LowKick, StrikesType.LowKick, StrikesType.Punch);
        mRules[13] = new StrikesRule(StrikesType.LowKick, StrikesType.LowKick, StrikesType.LowKick);
        mRules[14] = new StrikesRule(StrikesType.LowKick, StrikesType.LowKick, StrikesType.HighKick);
        mRules[15] = new StrikesRule(StrikesType.LowKick, StrikesType.HighKick, StrikesType.Punch);
        mRules[16] = new StrikesRule(StrikesType.LowKick, StrikesType.HighKick, StrikesType.LowKick);
        mRules[17] = new StrikesRule(StrikesType.LowKick, StrikesType.HighKick, StrikesType.HighKick);
        mRules[18] = new StrikesRule(StrikesType.HighKick, StrikesType.Punch, StrikesType.Punch);
        mRules[19] = new StrikesRule(StrikesType.HighKick, StrikesType.Punch, StrikesType.LowKick);
        mRules[20] = new StrikesRule(StrikesType.HighKick, StrikesType.Punch, StrikesType.HighKick);
        mRules[21] = new StrikesRule(StrikesType.HighKick, StrikesType.LowKick, StrikesType.Punch);
        mRules[22] = new StrikesRule(StrikesType.HighKick, StrikesType.LowKick, StrikesType.LowKick);
        mRules[23] = new StrikesRule(StrikesType.HighKick, StrikesType.LowKick, StrikesType.HighKick);
        mRules[24] = new StrikesRule(StrikesType.HighKick, StrikesType.HighKick, StrikesType.Punch);
        mRules[25] = new StrikesRule(StrikesType.HighKick, StrikesType.HighKick, StrikesType.LowKick);
        mRules[26] = new StrikesRule(StrikesType.HighKick, StrikesType.HighKick, StrikesType.HighKick);

        mWorkingMemory.strikeA = StrikesType.Unknown;
        mWorkingMemory.strikeB = StrikesType.Unknown;
        mWorkingMemory.strikeC = StrikesType.Unknown;

        mCount = mSuccessNum = mRandomSuccessNum = 0;
        mPreviousRuleIndex = -1;
        mRandomPrediction = StrikesType.Unknown;
        UnityEngine.Random.InitState(0);
	}

    private void Update()
    {
        labelPrediction.text = mWorkingMemory.strikeC.ToString();
        labelRandomPrediction.text = mRandomPrediction.ToString();
        labelCount.text = mCount.ToString();
        labelSuccess.text = mSuccessNum.ToString();
        labelRandomSuccess.text = mRandomSuccessNum.ToString();
    }
    void ProcessMove(StrikesType move)
    {
        if(mWorkingMemory.strikeA == StrikesType.Unknown)
        {
            mWorkingMemory.strikeA = move;
            return;
        }

        if(mWorkingMemory.strikeB == StrikesType.Unknown)
        {
            mWorkingMemory.strikeB = move;
            return;
        }

        //判断预测情况
        mCount++;
        if (move == mWorkingMemory.strikeC)
        {
            mSuccessNum++;
            if (mPreviousRuleIndex != -1)
                mRules[mPreviousRuleIndex].weight++;
        }
        else
        {
            if (mPreviousRuleIndex != -1)
                mRules[mPreviousRuleIndex].weight--;

            for(int i = 0; i < NUM_RULES; i++)
            {
                if(mRules[i].isMatched && (mRules[i].consequentC == move))
                {
                    mRules[i].weight++;
                    break;
                }
            }
        }

        if (move == mRandomPrediction)
            mRandomSuccessNum++;
        
        mWorkingMemory.strikeA = mWorkingMemory.strikeB;
        mWorkingMemory.strikeB = move;

        //进行下一轮预测
        for(int i = 0; i < NUM_RULES; i++)
        {
            if (mRules[i].antecedentA == mWorkingMemory.strikeA && mRules[i].antecedentB == mWorkingMemory.strikeB)
                mRules[i].isMatched = true;
            else
                mRules[i].isMatched = false;
        }

        int ruleIndex = -1;
        for(int i = 0; i < NUM_RULES; i++)
        {
            if(mRules[i].isMatched)
            {
                if (ruleIndex == -1 || (mRules[i].weight > mRules[ruleIndex].weight))
                    ruleIndex = i;
            }
        }

        if(ruleIndex != -1)
        {
            mWorkingMemory.strikeC = mRules[ruleIndex].consequentC;
            mPreviousRuleIndex = ruleIndex;
        }
        else
        {
            mWorkingMemory.strikeC = StrikesType.Unknown;
            mPreviousRuleIndex = -1;
        }

        mRandomPrediction = (StrikesType)Enum.Parse(typeof(StrikesType), ((int)UnityEngine.Random.Range(0, 3)).ToString());
    }    

    public void ClickBtnPunch()
    {
        ProcessMove(StrikesType.Punch);
    }

    public void ClickBtnLowKick()
    {
        ProcessMove(StrikesType.LowKick);
    }

    public void ClickBtnHighKick()
    {
        ProcessMove(StrikesType.HighKick);
    }
}
