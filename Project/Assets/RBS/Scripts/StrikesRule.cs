using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikesRule {
    public StrikesRule(StrikesType a, StrikesType b, StrikesType c)
    {
        antecedentA = a;
        antecedentB = b;
        consequentC = c;
        isMatched = false;
        weight = 0;
    }

    public StrikesType antecedentA;
    public StrikesType antecedentB;
    public StrikesType consequentC;
    public bool isMatched; //前两个是否吻合
    public int weight; //权重
}
