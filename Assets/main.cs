using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class main : MonoBehaviour {
	public GameObject cubePreFab;
	public GameObject jointPreFab;
	public GameObject hookPreFab;
	public GameObject jointPreFab2;
	public GameObject hookPreFab2;
	public GameObject wheel1PreFab;
	public GameObject wheel2PreFab;
	public static float timeLimit = 7.0f;
	public TextMesh generationN;
	public TextMesh averageFitnessN;
	public TextMesh top5FitnessN;
	public TextMesh globalMaxN;

	private static double top5Fitness;
	private static double averageFitness;

	private static GameObject[] cubes = new GameObject[180];
	private static GameObject[] joints = new GameObject[180];
	private static GameObject[] hooks = new GameObject[180];
	private static GameObject[] joints2 = new GameObject[180];
	private static GameObject[] hooks2 = new GameObject[180];
	private static GameObject[] wheel1s = new GameObject[180];
	private static GameObject[] wheel2s = new GameObject[180];
	private static float[] cubesStartX = new float[180];
	public static int inputN = 4;
	public static int outputN = 4;
	public static int populationSize = 120;
	
	//number of generations
	public static int loopN = 400;
	
	//number of species limits
	public static int maxSpeciesN = 30;
	public static int minSpeciesN = 5;

	//number of allowable generations for protecting innovation through speciation
	public static int allSpeciesLastLimit = 40;
	public static int speciesLastLimit = 10;
	public static double allSpeciesImprovementThreshold = 0.001;
	public static double speciesImprovementThreshold = 0.001;
	
	//species classification
	public static double weightWeight = 3.0;
	public static double excessWeight = 1.0;
	public static double disjointWeight = 1.0;
	//classification differences are adjusted to keep the number of different Species stable.
	public static double classificationDifferenceThreshold = 3.0;
	public static double currentCDT = 4.0;
	public static double cdtMultiplier = 0.1;
	
	//competition
	public static double speciesKeep = 0.40; //percent of networks in a species that are not killed each generation. These species are also the ones that breed the new generation.
	
	//all different types of mutation chances
	public static double mutateConnectionsChance = 0.25; //??
	public static double perturbChance = 0.85;
	public static double perturbPercent = 0.35;
	public static double perturbUniformChance = 0.7;
	public static double crossoverChance = 0.75;
	public static double linkMutationChance = 0.10;
	public static double nodeMutationChance = 0.08;
	public static double disableMutationKeep = 0.75;
	public static double enableMutationChance = 0.08;

	public static int interSpeciesBreedingN = 5;
	
	
	//number of top genomes in a species to calculate the average
	public static int averageFitnessGenomesN = 5;
	
	//number of top species
	public static int topDogKeep = 3;

	//number of top dogs
	public static int topDogParentKeep = 7;
	
	/// ///////////////////////////////////////////////////////////////////
	/// /// ///////////////////////////////////////////////////////////////
	/// /// ///////////////////////////////////////////////////////////////
	/// /// /// //////////////////Array Variable///////////////////////////
	/// /// /// //////////////////Initializations//////////////////////////
	/// /// /// ///////////////////////////////////////////////////////////
	/// /// /// ///////////////////////////////////////////////////////////
	/// /// /// ///////////////////////////////////////////////////////////
	/// /// /// ///////////////////////////////////////////////////////////
	
	public static double piOver180 = 0.01745329251;
	public static List<Network> networks;
	public static List<int> innovationInputs;
	public static List<int> innovationOutputs;
	public static int neuronN=0;
	//the first genome in a species list is the one used to compare for new genomes
	public static List<List<int>> species;
	
	
	
	public static List<double> speciesLastMaxFitness;
	public static List<int> speciesLastFitnessImprovement;
	
	public static double allSpeciesLastMaxFitness = -9999.0;
	public static int allSpeciesLastFitnessImprovement = 0;
	
	public static int startingIndexOfNewGenomes = 0;
	
	public static System.Random rand = new System.Random();
	public static double globalMax = 0.0;
	public static int finished;
	public static float timeCurrent = 0.0f;
	public static bool[] finishedFlag;
	public static int generation = 0;
	

	// Use this for initialization
	void Start () {
		//Initialize variables
		populationSetup ();
		
		//initialize gameobjects
		finishedFlag = new bool[populationSize];
		for (int i=0; i<populationSize; i++) {
			initEverything (i);
			finishedFlag[i] = false;
		}
		finished = 0;
		timeCurrent = 0.0f;

	}
	
	// Update is called once per frame
	void Update () {
		timeCurrent += Time.deltaTime;
		if (finished == 0) {
			for (int i=0; i<networks.Count; i++) {
				lukeUseTheForce (i);
			}

			//finished 1 generation
			if (timeCurrent > timeLimit) {
				finished = 1;
				for (int i=0; i<networks.Count; i++) {
					resetEverything(i);
				}

				loop ();

				for (int i=0; i<networks.Count; i++) {
					initEverything (i);
				}
				timeCurrent = 0.0f;
				generation++;
				Debug.Log (generation + ":" + globalMax);
				generationN.text = "Generation:" + (generation).ToString () + "\nAverage Fitness:" + averageFitness.ToString() + "\nTop 5 Fitness:" + top5Fitness.ToString()
					+ "\nTop Performance:" + globalMax.ToString() + "\nTime:" + timeLimit.ToString();
				/*globalMaxN.text = (globalMax).ToString ();
				top5FitnessN.text = top5Fitness.ToString ();
				averageFitnessN.text = averageFitness.ToString ();*/
				finished = 0;
			}
		}

		if (Input.GetKeyUp ("left")) {
			timeLimit -= 1.0f;
			generationN.text = "Generation:" + (generation).ToString () + "\nAverage Fitness:" + averageFitness.ToString() + "\nTop 5 Fitness:" + top5Fitness.ToString()
				+ "\nTop Performance:" + globalMax.ToString() + "\nTime:" + timeLimit.ToString();
			globalMax = 0.0;
		}
		if (Input.GetKeyUp ("right")) {
			timeLimit += 1.0f;
			generationN.text = "Generation:" + (generation).ToString () + "\nAverage Fitness:" + averageFitness.ToString() + "\nTop 5 Fitness:" + top5Fitness.ToString()
				+ "\nTop Performance:" + globalMax.ToString() + "\nTime:" + timeLimit.ToString();
			globalMax = 0.0;
		}

	}
	public void initEverything(int index){
		int x = index / 13;
		int z = index % 13;
		float sx = ((float)x) * 9.0f;
		float sz = 0.0f;
		if (x % 2 == 0) {
			sz = ((float)z) * 4.5f;
		}
		else if (x % 2 == 1) {
			sz = ((float)z) * 4.5f + 2.25f;
		}


		cubes[index] = Instantiate (cubePreFab) as GameObject;
		joints[index] = Instantiate (jointPreFab) as GameObject;
		hooks [index] = Instantiate (hookPreFab) as GameObject;
		joints2[index] = Instantiate (jointPreFab2) as GameObject;
		hooks2 [index] = Instantiate (hookPreFab2) as GameObject;
		wheel1s [index] = Instantiate (wheel1PreFab) as GameObject;
		wheel2s [index] = Instantiate (wheel2PreFab) as GameObject;

		/*int posneg = rand.Next (0, 2);
		float tchange = 0.0f;
		if (posneg == 0) {
			tchange = 0.02f;
		} else {
			tchange = -0.02f;
		}*/
		cubesStartX [index] = cubes [index].transform.position.x + sx;
		cubes[index].transform.position = new Vector3 (cubes[index].transform.position.x + sx, cubes[index].transform.position.y, cubes[index].transform.position.z + sz);
		joints [index].transform.position = new Vector3 (joints[index].transform.position.x + sx, joints[index].transform.position.y, joints[index].transform.position.z + sz);
		hooks [index].transform.position = new Vector3 (hooks[index].transform.position.x + sx, hooks[index].transform.position.y, hooks[index].transform.position.z + sz);
		joints2 [index].transform.position = new Vector3 (joints2[index].transform.position.x + sx, joints2[index].transform.position.y, joints2[index].transform.position.z + sz);
		hooks2 [index].transform.position = new Vector3 (hooks2[index].transform.position.x + sx, hooks2[index].transform.position.y, hooks2[index].transform.position.z + sz);
		wheel1s [index].transform.position = new Vector3 (wheel1s[index].transform.position.x + sx, wheel1s[index].transform.position.y, wheel1s[index].transform.position.z + sz);
		wheel2s [index].transform.position = new Vector3 (wheel2s[index].transform.position.x + sx, wheel2s[index].transform.position.y, wheel2s[index].transform.position.z + sz);

		joints [index].AddComponent <HingeJoint>();
		joints[index].GetComponent<HingeJoint>().connectedBody = cubes [index].GetComponent<Rigidbody> ();
		joints[index].GetComponent<HingeJoint>().anchor = new Vector3 (0.0f, -1.0f, 0.0f);
		joints[index].GetComponent<HingeJoint>().axis = new Vector3 (0.0f, 0.0f, 1.0f);
		joints[index].GetComponent<HingeJoint>().autoConfigureConnectedAnchor = true;
		joints[index].GetComponent<HingeJoint>().useMotor = true;
		JointMotor jm = new JointMotor();
		jm.force = 2000.0f;
		jm.targetVelocity = 0.0f;
		joints[index].GetComponent<HingeJoint>().motor = jm;

		joints2 [index].AddComponent <HingeJoint>();
		joints2[index].GetComponent<HingeJoint>().connectedBody = cubes [index].GetComponent<Rigidbody> ();
		joints2[index].GetComponent<HingeJoint>().anchor = new Vector3 (0.0f, -1.0f, 0.0f);
		joints2[index].GetComponent<HingeJoint>().axis = new Vector3 (0.0f, 0.0f, 1.0f);
		joints2[index].GetComponent<HingeJoint>().autoConfigureConnectedAnchor = true;
		joints2[index].GetComponent<HingeJoint>().useMotor = true;
		JointMotor jm2 = new JointMotor();
		jm2.force = 2000.0f;
		jm2.targetVelocity = 0.0f;
		joints2[index].GetComponent<HingeJoint>().motor = jm2;

		hooks [index].AddComponent<HingeJoint> ();
		hooks[index].GetComponent<HingeJoint>().connectedBody = joints [index].GetComponent<Rigidbody> ();
		hooks[index].GetComponent<HingeJoint>().anchor = new Vector3 (0.0f, 1.0f, 0.0f);
		hooks[index].GetComponent<HingeJoint>().axis = new Vector3 (0.0f, 0.0f, 1.0f);
		hooks[index].GetComponent<HingeJoint>().autoConfigureConnectedAnchor = true;
		hooks[index].GetComponent<HingeJoint>().useMotor = true;

		JointMotor hm = new JointMotor();
		hm.force = 2000.0f;
		hm.targetVelocity = 0.0f;
		hooks[index].GetComponent<HingeJoint>().motor = hm;

		hooks2 [index].AddComponent<HingeJoint> ();
		hooks2[index].GetComponent<HingeJoint>().connectedBody = joints2 [index].GetComponent<Rigidbody> ();
		hooks2[index].GetComponent<HingeJoint>().anchor = new Vector3 (0.0f, 1.0f, 0.0f);
		hooks2[index].GetComponent<HingeJoint>().axis = new Vector3 (0.0f, 0.0f, 1.0f);
		hooks2[index].GetComponent<HingeJoint>().autoConfigureConnectedAnchor = true;
		hooks2[index].GetComponent<HingeJoint>().useMotor = true;
		
		JointMotor hm2 = new JointMotor();
		hm2.force = 2000.0f;
		hm2.targetVelocity = 0.0f;
		hooks2[index].GetComponent<HingeJoint>().motor = hm2;

		wheel1s [index].AddComponent <HingeJoint>();
		wheel1s[index].GetComponent<HingeJoint>().connectedBody = cubes [index].GetComponent<Rigidbody> ();
		wheel1s[index].GetComponent<HingeJoint>().anchor = new Vector3 (0.0f, 0.0f, 0.0f);
		wheel1s[index].GetComponent<HingeJoint>().axis = new Vector3 (0.0f, 1.0f, 0.0f);
		wheel1s[index].GetComponent<HingeJoint>().autoConfigureConnectedAnchor = true;

		wheel2s [index].AddComponent <HingeJoint>();
		wheel2s[index].GetComponent<HingeJoint>().connectedBody = cubes [index].GetComponent<Rigidbody> ();
		wheel2s[index].GetComponent<HingeJoint>().anchor = new Vector3 (0.0f, 0.0f, 0.0f);
		wheel2s[index].GetComponent<HingeJoint>().axis = new Vector3 (0.0f, 1.0f, 0.0f);
		wheel2s[index].GetComponent<HingeJoint>().autoConfigureConnectedAnchor = true;
	}
	public void resetEverything(int index){
		Destroy (cubes [index]);
		Destroy (joints [index]);
		Destroy (hooks [index]);
		Destroy (joints2 [index]);
		Destroy (hooks2 [index]);
		Destroy (wheel1s [index]);
		Destroy (wheel2s [index]);
	}

	public static void populationSetup(){
		finished = 0;
		networks = new List<Network> ();
		innovationInputs = new List<int> ();
		innovationOutputs = new List<int> ();
		for(int i=0;i<populationSize;i++){
			Network net = new Network();
			
			//add xorFitnessinputs
			for(int j=0;j<inputN;j++){
				Neuron n = new Neuron();
				net.addNeuron(n);
				net.addInput(j);
			}
			
			//add outputs
			for(int j=0;j<outputN;j++){
				Neuron n = new Neuron();
				net.addNeuron(n);
				net.addOutput(inputN + j);
			}
			neuronN = inputN + outputN;
			//add edges to outputs
			for(int j=0;j<inputN;j++){
				for(int k=0;k<outputN;k++){
					//add a random weight between -2 and 2
					bool temp = net.addEdge(j, k+inputN, randomEdgeWeight(), getInnovation(j, k+inputN));
				}
			}
			
			networks.Add(net);
		}
		
		startingIndexOfNewGenomes = 0;
		
		//initialize species
		species = new List<List<int>> ();
		speciesLastMaxFitness = new List<double> ();
		speciesLastFitnessImprovement = new List<int> ();
	}
	
	//One simulation loop (one generation)
	public static void loop(){
		//calculate networks' fitness
		for (int i=0; i<networks.Count; i++) {
			networks[i].fitness = crawlFitness(i);
		}

		//split genomes into species
		for(int i=startingIndexOfNewGenomes;i<networks.Count;i++){
			int speciesIndex = -1;
			for(int j=0;j<species.Count;j++){
				if(classification(i, species[j][0]) < currentCDT){
					speciesIndex = j;
					break;
				}
			}
			
			//did not find similar species
			if(speciesIndex == -1){
				//add to new species
				List<int> tList = new List<int>();
				tList.Add(i);
				species.Add(tList);
				speciesLastMaxFitness.Add(-9999.0);
				speciesLastFitnessImprovement.Add(0);
			}
			else{
				species[speciesIndex].Add(i);
			}
		}
		
		//adjust classificationDifferentThreshold
		if (species.Count < minSpeciesN) {
			currentCDT *= (1.0 + cdtMultiplier);
		}
		if (species.Count > maxSpeciesN) {
			currentCDT *= (1.0 - cdtMultiplier);
		}
		

		
		//order the fitness of networks within species
		for(int i=0;i<species.Count;i++){
			for(int j=0;j<species[i].Count;j++){
				double max = -9999.0;
				int maxIndex = 0;
				for(int k = j;k<species[i].Count;k++){
					if(networks[species[i][k]].fitness > max){
						max = networks[species[i][k]].fitness;
						maxIndex = k;
					}
				}
				//swap max with bottom
				int temp = species[i][j];
				species[i][j] = species[i][maxIndex];
				species[i][maxIndex] = temp;
			}
		}
		
		int remainingNumberOfGenomes = 0;
		//delete poor performing networks in a species
		for (int i=0; i<species.Count; i++) {
			int speciesDeleted = (int)(((double) species[i].Count) * (1.0 - speciesKeep));
			for (int j=0; j<speciesDeleted; j++) {
				species[i].RemoveAt(species[i].Count - 1);
			}
			remainingNumberOfGenomes += species[i].Count;
		}
		
		
		double populationAverageFitness = 0.0;
		//calculate the average fitness of species
		List<double> speciesAverageFitness = new List<double> ();
		for (int i=0; i<species.Count; i++) {
			double sum = 0.0;
			int tN = smaller(species[i].Count, averageFitnessGenomesN);
			for(int j=0;j<tN;j++){
				sum += networks[species[i][j]].fitness;
			}
			populationAverageFitness += sum;
			speciesAverageFitness.Add(sum / ((double)tN) + networks[species[i][0]].fitness);
		}
		
		populationAverageFitness /= ((double)networks.Count);
		averageFitness = populationAverageFitness;
		if ((populationAverageFitness / allSpeciesLastMaxFitness) > 1.0 + allSpeciesImprovementThreshold) {
			allSpeciesLastFitnessImprovement = 0;
			allSpeciesLastMaxFitness = populationAverageFitness;
		} else {
			allSpeciesLastFitnessImprovement++;
		}
		
		//average population has not improved for a while, kill off all but top 2 species
		/*if (allSpeciesLastFitnessImprovement > allSpeciesLastLimit) {
				for(int i=2;i<species.Count;i++){
					species.RemoveAt(i);
					speciesAverageFitness.RemoveAt(i);
					speciesLastMaxFitness.RemoveAt(i);
					speciesLastFitnessImprovement.RemoveAt(i);
					i--;
				}
			}*/
		
		List<int> topDogs = new List<int>();
		
		while (topDogs.Count < topDogKeep && topDogs.Count < species.Count) {
			double tMaxFitness = -999.9;
			int tMaxIndex = -1;
			for (int i=0; i<species.Count; i++) {
				bool tflag = false;
				for(int j=0;j<topDogs.Count;j++){
					if(i == topDogs[j]){
						tflag = true;
						break;
					}
				}
				if(tflag){
					continue;
				}
				if(networks[species[i][0]].fitness > tMaxFitness){
					tMaxFitness = networks[species[i][0]].fitness;
					tMaxIndex = i;
				}
				
			}
			topDogs.Add(tMaxIndex);
		}
		
		
		for (int i=0; i<species.Count; i++) {
			bool tflag = false;
			for(int j=0;j<topDogs.Count;j++){
				if(i == topDogs[j]){
					tflag = true;
					break;
				}
			}
			if(tflag){
				speciesLastFitnessImprovement[i] = 0;
				continue;
			}
			//speciesLastFitnessImprovement.Add(0);
			if(speciesAverageFitness[i] > speciesLastMaxFitness[i]){
				speciesLastMaxFitness[i] = speciesAverageFitness[i];
			}
			else{
				speciesLastFitnessImprovement[i]++;
				//remove species if they fail too much
				if(speciesLastFitnessImprovement[i] > speciesLastLimit){
					species.RemoveAt(i);
					speciesAverageFitness.RemoveAt(i);
					speciesLastMaxFitness.RemoveAt(i);
					speciesLastFitnessImprovement.RemoveAt(i);
					i--;
				}
			}
		}
		
		//get ranking of species
		int[] speciesRanking = new int[species.Count];
		for (int i=0; i<species.Count; i++) {
			speciesRanking [i] = -1;
		}
		
		for (int i=0; i<species.Count; i++) {
			double maxSpeciesFitness = -9999.0;
			int maxSpeciesIndex = -1;
			for (int j=0; j<species.Count; j++) {
				if(speciesRanking[j] == -1){
					if(speciesAverageFitness[j] > maxSpeciesFitness){
						maxSpeciesFitness = speciesAverageFitness[j];
						maxSpeciesIndex = j;
					}
				}
			}
			speciesRanking[maxSpeciesIndex] = species.Count - i;
		}
		
		double maxFitness = 0.0;
		int tmaxIndex = 0;
		for (int i=0; i<species.Count; i++) {
			if (networks [species [i] [0]].fitness > maxFitness) {
				maxFitness = networks [species [i] [0]].fitness;
				tmaxIndex = i;
			}
		}
		if (maxFitness > 7.0f) {
			networks [species [tmaxIndex] [0]].printNetwork();
		}
		if(maxFitness > globalMax){
			globalMax = maxFitness;
		}
		Debug.Log ("maxfitness:" + maxFitness);
		double numberOfChildren = ((double)populationSize - ((double) remainingNumberOfGenomes));
		List<Network> childrenGenomes = new List<Network> ();
		//breed new genomes
		for (int i=0; i<species.Count; i++) {
			int speciesChildrenN = getChildrenN((double)speciesRanking[i], (double)species.Count, numberOfChildren);
			for(int j=0;j<speciesChildrenN;j++){
				int parent0 = getParent(species[i].Count);
				int parent1 = getParent(species[i].Count); 
				Network childNet;
				if(parent0 < parent1){
					childNet = breed(species[i][parent0], species[i][parent1]);
				}
				else{
					childNet = breed(species[i][parent1], species[i][parent0]);
				}
				
				
				//add new genome to childrenGenomes
				childrenGenomes.Add(childNet);
			}
		}

		//Breed two different species together
		if (species.Count > 3) {
			int[] speciesBreed1 = new int[5]{0,1,0,0,1};
			int[] speciesBreed2 = new int[5]{1,2,2,3,3};
			for (int i=0; i<interSpeciesBreedingN; i++) {
				Network childNet = breed(species[speciesBreed1[i]][0], species[speciesBreed2[i]][0]);
				childrenGenomes.Add(childNet);
			}
		}
		//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
		//remove deleted genomes and update species list
		//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
		List<Network> parentGenomes = new List<Network> ();
		List<List<int>> newSpecies = new List<List<int>> ();
		for (int i=0; i<species.Count; i++) {
			List<int> tList = new List<int>();
			newSpecies.Add(tList);
			for(int j=0;j<species[i].Count;j++){
				parentGenomes.Add(networks[species[i][j]]);
				newSpecies[i].Add(parentGenomes.Count-1);
			}
		}
		
		//don't perturb top 5 parents
		List<int> topDogParents = new List<int>();
		
		while (topDogParents.Count < topDogParentKeep) {
			double tMaxFitness = -999.9;
			int tMaxIndex = -1;
			for (int i=0; i<parentGenomes.Count; i++) {
				bool tflag = false;
				for(int j=0;j<topDogParents.Count;j++){
					if(i == topDogParents[j]){
						tflag = true;
						break;
					}
				}
				if(tflag){
					continue;
				}
				if(parentGenomes[i].fitness > tMaxFitness){
					tMaxFitness = parentGenomes[i].fitness;
					tMaxIndex = i;
				}
				
			}
			topDogParents.Add(tMaxIndex);
		}

		double topAverage = 0.0;
		for (int i=0; i<topDogParents.Count; i++) {
			topAverage += parentGenomes[topDogParents[i]].fitness;
		}
		top5Fitness = topAverage / ((double)topDogParents.Count);
		
		//pertub the parents
		for (int i=0; i<parentGenomes.Count; i++) {
			bool tflag = false;
			for(int j=0;j<topDogParents.Count;j++){
				if(i == topDogParents[j]){
					tflag = true;
					break;
				}
			}
			if(tflag){
				continue;
			}
			double rand01 = rand.NextDouble ();
			if (rand01 < perturbChance) {
				double perturbAmount = (rand.NextDouble () * perturbPercent * 2.0 - perturbPercent);
				for(int j=0;j<parentGenomes[i].edges.Count;j++){
					//perturb it! sore aru
					if(rand.NextDouble() < perturbUniformChance){
						parentGenomes[i].edges[j].weight = parentGenomes[i].edges[j].weight + perturbAmount;
					}
					else{
						//give weight a new value;
						parentGenomes[i].edges[j].weight = randomEdgeWeight();
					}
					if(!parentGenomes[i].edges[j].isEnabled){
						rand01 = rand.NextDouble();
						if(rand01 < enableMutationChance){
							parentGenomes[i].edges[j].isEnabled = true;
						}
					}
				}
			}
		}
		
		
		species = newSpecies;
		networks = parentGenomes;
		startingIndexOfNewGenomes = networks.Count;
		for (int i=0; i<childrenGenomes.Count; i++) {
			networks.Add (childrenGenomes [i]);
		}

		
	}
	
	private static Network breed(int parentA, int parentB)
	{
		List<Edge> childEdges = new List<Edge> ();
		int AIndex = 0;
		int BIndex = 0;
		
		//merge genomes
		while (AIndex < networks[parentA].edges.Count && BIndex < networks[parentB].edges.Count) {
			if (networks [parentA].edges [AIndex].innovation == networks [parentB].edges [BIndex].innovation) {
				if (rand.Next (0, 2) == 0) {
					Edge e = new Edge(networks[parentA].edges[AIndex].inNeuron,networks[parentA].edges[AIndex].outNeuron,networks[parentA].edges[AIndex].weight, networks[parentA].edges[AIndex].isEnabled,networks[parentA].edges[AIndex].innovation);
					childEdges.Add (e);
				} else {
					Edge e = new Edge(networks[parentB].edges[BIndex].inNeuron,networks[parentB].edges[BIndex].outNeuron,networks[parentB].edges[BIndex].weight, networks[parentB].edges[BIndex].isEnabled,networks[parentB].edges[BIndex].innovation);
					childEdges.Add (e);
				}
				AIndex++;
				BIndex++;
			} else if (networks [parentA].edges [AIndex].innovation > networks [parentB].edges [BIndex].innovation) {
				BIndex++;
			} else {
				Edge e = new Edge(networks[parentA].edges[AIndex].inNeuron,networks[parentA].edges[AIndex].outNeuron,networks[parentA].edges[AIndex].weight, networks[parentA].edges[AIndex].isEnabled,networks[parentA].edges[AIndex].innovation);
				childEdges.Add (e);
				AIndex++;
			}
		}
		//excess edges left over from dominant parent
		if (BIndex == networks [parentB].edges.Count) {
			for (int i=AIndex; i<networks[parentA].edges.Count; i++) {
				Edge e = new Edge(networks[parentA].edges[i].inNeuron,networks[parentA].edges[i].outNeuron,networks[parentA].edges[i].weight, networks[parentA].edges[i].isEnabled,networks[parentA].edges[i].innovation);
				childEdges.Add (e);
			}
		}
		
		//perturb it
		double rand01 = rand.NextDouble ();
		if (rand01 < perturbChance) {
			double perturbAmount = (rand.NextDouble () * perturbPercent * 2.0 - perturbPercent);
			for (int i=0; i<childEdges.Count; i++) {
				//perturb it! sore aru
				if(rand.NextDouble() < 0.9){
					childEdges [i].weight = childEdges [i].weight + perturbAmount;
				}
				else{
					//give weight a new value;
					childEdges [i].weight = randomEdgeWeight();
				}
				
				if (!childEdges [i].isEnabled) {
					rand01 = rand.NextDouble ();
					if (rand01 < enableMutationChance) {
						childEdges [i].isEnabled = true;
					}
				}
			}
		}
		
		
		//create the child network
		Network net = new Network ();
		//add inputs
		for(int j=0;j<inputN;j++){
			Neuron n = new Neuron();
			net.addNeuron(n);
			net.addInput(j);
		}
		
		//add outputs
		for(int j=0;j<outputN;j++){
			Neuron n = new Neuron();
			net.addNeuron(n);
			net.addOutput(inputN + j);
		}
		for (int j=0; j<(neuronN - inputN - outputN); j++) {
			Neuron n = new Neuron ();
			net.addNeuron (n);
		}
		
		for(int i=0;i<childEdges.Count;i++){
			bool flag = net.addEdge(childEdges[i].inNeuron, childEdges[i].outNeuron, childEdges[i].weight, childEdges[i].innovation);
			if(flag==false){
				//Console.WriteLine("wtf add edge failed");
			}
		}
		
		
		//new links
		double rand2 = rand.NextDouble();
		if (rand2 < linkMutationChance) {
			int tCount = 0;
			while(tCount <30){
				int inputNode = rand.Next(0,neuronN - outputN);
				if(inputNode >= inputN){
					inputNode += outputN;
				}
				int outputNode = rand.Next (inputN, neuronN);
				if(net.checkEdge(inputNode, outputNode)){
					//add edge if the edge is connected to the input. 
					if(net.neurons[inputNode].inputEdges.Count > 0 || inputNode < inputN){
						bool tb = net.addEdge(inputNode, outputNode, randomEdgeWeight(), getInnovation(inputNode, outputNode));
						if(tb){
							tCount = 20;
						}
					}
				}
				tCount++;
			}
		}
		
		//new node
		rand2 = rand.NextDouble ();
		if (rand2 < nodeMutationChance) {
			int tCount = 0;
			while(tCount < 10){
				int randomEdge = rand.Next(0,net.edges.Count);
				bool flag = true;
				int randomEdgeIn = net.edges[randomEdge].inNeuron;
				int randomEdgeOut = net.edges[randomEdge].outNeuron;
				//check if there already exists a node that does the same thing
				for(int i=0;i<innovationInputs.Count;i++){
					if(innovationInputs[i] == randomEdgeIn){
						for(int j=0;j<innovationInputs.Count;j++){
							if(innovationInputs[j] == innovationOutputs[i]){
								if(innovationOutputs[j] == randomEdgeOut){
									flag = false;
								}
							}
						}
					}
				}
				
				//create new node
				if(flag){
					net.edges[randomEdge].isEnabled = false;
					Neuron tN = new Neuron();
					net.addNeuron(tN);
					
					bool tb = net.addEdge(net.edges[randomEdge].inNeuron, neuronN, randomEdgeWeight(), getInnovation(net.edges[randomEdge].inNeuron, neuronN));
					bool tc = net.addEdge(neuronN, net.edges[randomEdge].outNeuron, randomEdgeWeight(), getInnovation(neuronN, net.edges[randomEdge].outNeuron));
					if(!tb || !tc){
						//Console.WriteLine("add Edge failed");
					}
					neuronN++;
					tCount = 20;
					net.printNetwork();
				}
				tCount++;
			}
			
		}
		
		return net;
	}
	
	
	//returns how far apart the two genomes are
	private static double classification(int netIndexA, int netIndexB){
		int AIndex = 0;
		int BIndex = 0;
		double disjointN = 0.0;
		double excessN = 0.0;
		double weightDifference = 0.0;
		while (AIndex < networks[netIndexA].edges.Count && BIndex < networks[netIndexB].edges.Count) {
			if(networks[netIndexA].edges[AIndex].innovation == networks[netIndexB].edges[BIndex].innovation){
				weightDifference += Math.Abs(networks[netIndexA].edges[AIndex].weight - networks[netIndexB].edges[BIndex].weight);
				AIndex++;
				BIndex++;
			}
			else if(networks[netIndexA].edges[AIndex].innovation > networks[netIndexB].edges[BIndex].innovation){
				disjointN += 1.0;
				BIndex++;
			}
			else{
				disjointN += 1.0;
				AIndex++;
			}
		}
		if (AIndex == networks [netIndexA].edges.Count) {
			excessN = networks [netIndexB].edges.Count - BIndex;
		} else if (BIndex == networks [netIndexB].edges.Count) {
			excessN = networks [netIndexA].edges.Count - AIndex;
		} else {
			//Console.WriteLine("network classification error");
		}
		
		return ((weightWeight * weightDifference) + (disjointWeight * disjointN + excessWeight * excessN) / bigger (networks [netIndexA].edges.Count, networks [netIndexB].edges.Count));
	}
	
	//keeps indexes on the innovation count of each gene. New genes increase innovation
	private static int getInnovation(int input, int output){
		for (int i=0; i<innovationInputs.Count; i++) {
			if(input == innovationInputs[i]){
				if(output == innovationOutputs[i])
				{
					return i;
				}
			}
		}
		innovationInputs.Add (input);
		innovationOutputs.Add (output);
		return (innovationInputs.Count - 1);
	}

	private static double crawlFitness(int index){
		return (cubes [index].transform.position.x - cubesStartX [index]);
	}

	
	private static int getParent(int total){
		int totalRandoms = 0;
		if (total % 2 == 0) {
			totalRandoms = (total / 2) * (total + 1);
		} else {
			totalRandoms = ((total+1)/2) * total;
		}
		totalRandoms += totalRandoms / 4;
		int randomInt = rand.Next (1, totalRandoms+1);
		int sum = 0;
		for (int i=1; i<total+1; i++) {
			sum += i;
			if (randomInt <= sum) {
				return (total - i);
			}
		}
		return 0;
	}
	
	private static int getChildrenN(double ranking, double total, double totalChildren){
		return (int)(ranking / (total * (total + 1.0) / 2.0) * totalChildren);
	}
	
	
	//calculates the fitness of a single xor case
	private static double xorFitnessSingle(double inputA, double inputB, double output)
	{
		double answer = inputA + inputB;
		if (answer > 1.1) {
			answer = 0.0;
		}
		return 1.0 / (1.0 + Math.Abs (answer - output));
	}
	
	private static double hyperbolicTangent(double x)
	{
		double e2z = Math.Pow (Math.E, 2.0 * x);
		return (e2z - 1.0) / (e2z + 1.0);
	}
	
	private static int bigger(int a, int b){
		if (a > b) {
			return a;
		}
		return b;
	}
	private static int smaller(int a, int b){
		if (a < b) {
			return a;
		}
		return b;
	}
	private static void lukeUseTheForce(int index){
		//use angle of the joints and hook, or angle of the motors
		List<double> inputs = new List<double> ();
		inputs.Add ((double)(joints[index].transform.eulerAngles.z * piOver180));
		inputs.Add ((double)(hooks[index].transform.eulerAngles.z * piOver180));
		inputs.Add ((double)(joints2[index].transform.eulerAngles.z * piOver180));
		inputs.Add ((double)(hooks2[index].transform.eulerAngles.z * piOver180));
		//inputs.Add ((double)(topCyl [index].transform.position.y - botCyl [index].transform.position.y));

		List<double> outputs = networks [index].calculateOutput (inputs);
		//botCyl [index].GetComponent<Rigidbody> ().velocity = new Vector3 ((float)outputs [0], 0.0f, 0.0f);
		JointMotor jm = new JointMotor();
		jm.force = 3000.0f;
		jm.targetVelocity = 60.0f * ((float)outputs[0]);

		JointMotor hm = new JointMotor();
		hm.force = 3000.0f;
		hm.targetVelocity = 60.0f * ((float)outputs[1]);

		joints[index].GetComponent<HingeJoint>().motor = jm;
		hooks[index].GetComponent<HingeJoint>().motor = hm;

		JointMotor jm2 = new JointMotor();
		jm2.force = 3000.0f;
		jm2.targetVelocity = 60.0f * ((float)outputs[2]);
		
		JointMotor hm2 = new JointMotor();
		hm2.force = 3000.0f;
		hm2.targetVelocity = 60.0f * ((float)outputs[3]);
		
		joints2[index].GetComponent<HingeJoint>().motor = jm2;
		hooks2[index].GetComponent<HingeJoint>().motor = hm2;
	}
	
	private static double randomEdgeWeight(){
		return ((rand.NextDouble ()*4.0) - 2.0);
		//return hyperbolicTangent (rand.NextDouble () * 4.0 - 2.0);
	}
}
