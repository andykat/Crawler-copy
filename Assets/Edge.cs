using UnityEngine;
using System;
using System.Collections;
public class Edge{
	public int inNeuron;
	public double weight;
	public int outNeuron;
	public bool isEnabled;
	public int innovation;
	public Edge(int tInNeuron, int tOutNeuron, double tWeight, bool tIsEnabled, int tInnovation){
		inNeuron = tInNeuron;
		outNeuron = tOutNeuron;
		weight = tWeight;
		isEnabled = tIsEnabled;
		innovation = tInnovation;
	}
}
