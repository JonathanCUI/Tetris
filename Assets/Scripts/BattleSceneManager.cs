using UnityEngine;
using Tetris;
using System.Collections.Generic;
using DG.Tweening;

enum BasicShape
{
	I,
	J,
	L,
	O,
	S,
	Z,
	T
}


public class BattleSceneManager : MonoBehaviour {

	//data members
	private const int MAP_ROW_COUNT = 20;
	private const int MAP_COL_COUNT = 10;
	//map layout details
	private Vector3 _leftUpPosition = new Vector3(-4.95f, 4.96f, 0f);
	private const float UNIT_LENGTH = 0.75f;
	private const float UNIT_SCALE = 0.7f;

    //next ones details
    private Vector3 _nextLeftUpPosition = new Vector3(3f, 4.37f, -2f);
    private const float NEXT_UNIT_LENGTH = 0.53f;
    private const float NEXT_UNIT_SCALE  = 0.5f;
    private const float NEXT_UNIT_GAP    = 3f;

	//战场信息，false代表没有被占，而true代表已经被占
	private bool[, ] _stableMatrix = new bool[MAP_ROW_COUNT, MAP_COL_COUNT];
    private BaseShape _currentShape;
    private Vector2 _currentShapeOrigin;
	private List<Vector2> _currentDownList = new List<Vector2>();	    //当前下落块的行列索引列表
	private List<GameObject> _squareList   = new List<GameObject>();    //整个地图列表
    private List<GameObject> _nextOnesSquareList = new List<GameObject>(); //接下来出现图形的方块列表
    private List<Color> _randomColorList   = new List<Color>();         //随机颜色列表
    private List<BaseShape> _nextOnesList = new List<BaseShape>();    //接下来将要出现的图形索引


	//自由下落时间累积
	private float _fallDownTimeAccumulation;
	private float _fallDownTimeLimit = 0.5f;

	//左右移动速度
	private float _horizonTimeLimit = 0.1f;
	private float _leftTimeAccumulation;
	private float _rightTimeAccumulation;
    private bool _needRestart;
    private Color _currentDownColor;

	//存放已经满行的行索引，用来消除和计分
	List<int> _fullRowIndexList = new List<int>();

	// Use this for initialization
	void Start () 
	{
        //构建战场矩阵Matrix
        for (int row = 0; row < MAP_ROW_COUNT; row++) 
        {
            for (int col = 0; col < MAP_COL_COUNT; col++) 
            {
                GameObject go = Instantiate (Resources.Load ("Prefabs/Unit") as GameObject);
                go.transform.localScale = Vector3.one * UNIT_SCALE;
                go.transform.position = GetPositionByIndex (row, col);
                _squareList.Add(go);
            }
        }
        //构建随机颜色库
        _randomColorList.Add(Color.red);
        _randomColorList.Add(Color.green);
        _randomColorList.Add(Color.blue);
        _randomColorList.Add(Color.yellow);
        _randomColorList.Add(Color.cyan);
        _randomColorList.Add(Color.magenta);

        //构建接下来将出现的图形库，这里显示4组将要出现的图形
        for(int index = 0; index < 4; index++)
        {
            for(int row = 0; row < 4; row++)
            {
                for(int col = 0; col < 4; col++)
                {
                    GameObject go = Instantiate(Resources.Load("Prefabs/Unit") as GameObject);
                    go.transform.localScale = Vector3.one * NEXT_UNIT_SCALE;
                    go.transform.position = GetNextPositionByIndexes(index, row, col);
                    go.GetComponent<Renderer>().material.color = Color.black;
                    _nextOnesSquareList.Add(go);
                }
            }
        }

        ShapeManager.Initialize();
        InitialMap();
        _currentShape = null;
		//初始化参数
		_fallDownTimeAccumulation = 0f;

        _needRestart = false;
	}
	
    private void InitialMap()
    {       
        //initial the map
        for (int row = 0; row < MAP_ROW_COUNT; row++) 
        {
            for (int col = 0; col < MAP_COL_COUNT; col++) 
            {
                _stableMatrix [row, col] = false;
            }
        }

        for(int i = 0; i < _squareList.Count; i++)
        {
            _squareList[i].SetActive(false);
        }
        UIManager.Instance.GameMessageText.text = "";

        //初始化4个接下来将要出现的图形
        for(int i = 0; i < 4; i++)
        {
            BaseShape bs = ShapeManager.Instance.GetNextShape();
            bs.ResetStartShapeAndColor();
            _nextOnesList.Add(bs);
        }
    }


	// Update is called once per frame
	void Update () 
	{	
        if(_needRestart)
        {
            if(Input.GetKeyDown(KeyCode.R))
            {
                InitialMap();
                _currentShape = null;
                _needRestart = false;
            }
            else
            {
                return;
            }
        }

        if (_currentShape == null) 
        {
			_currentDownList.Clear ();
            _currentShape = _nextOnesList[0];
            _currentDownColor = _currentShape.ShapeColor;
            _currentDownList = _currentShape.GetMapDownSquareListByOrigin(_currentShape.StartOffSet);
            _currentShapeOrigin = _currentShape.StartOffSet;

            //产生一个新的图形来补充到接下来的图形列表当中
            _nextOnesList.RemoveAt(0);
            BaseShape bs = ShapeManager.Instance.GetNextShape();
            bs.ResetStartShapeAndColor();
            _nextOnesList.Add(bs);

            //刷新接下来图形的显示界面
            //将所有的接下来形状显示区域刷新为黑色
            for(int i = 0; i < _nextOnesSquareList.Count; i++)
            {
                _nextOnesSquareList[i].GetComponent<Renderer>().material.color = Color.black;
            }
            //将每一个接下来图形显示出来
            for(int index = 0; index < 4; index++)
            {
                List<Vector2> shapeList = _nextOnesList[index].GetMapDownSquareListByOrigin(Vector2.zero);
                for(int i = 0; i < shapeList.Count; i++)
                {
                    GetNextSquareByIndex(index, (int)shapeList[i].x, (int)shapeList[i].y).GetComponent<Renderer>().material.color = _nextOnesList[index].ShapeColor;
                }
            }


            for(int i = 0; i < _currentDownList.Count; i++)
            {
                if((int)_currentDownList[i].x >= 0)
                {
                    if(CheckValidByIndex((int)_currentDownList[i].x, (int)_currentDownList[i].y))
                    {
                        GetSquareByIndex ((int)_currentDownList[i].x, (int)_currentDownList[i].y).SetActive (true);
                        GetSquareByIndex((int)_currentDownList[i].x, (int)_currentDownList[i].y).GetComponent<Renderer>().material.color = _currentDownColor;
                    }
                    else
                    {
                        Debug.Log("Game Over");
                        UIManager.Instance.GameMessageText.text = "Game Over";
                        _needRestart = true;
                        return;
                    }
                }
            }

			_fallDownTimeAccumulation = 0f;
		}
		else 
		{
			_fallDownTimeAccumulation += Time.deltaTime;
			if(_fallDownTimeAccumulation > _fallDownTimeLimit)
			{
				_fallDownTimeAccumulation -= _fallDownTimeLimit;
				//检查碰撞
				if (!CheckDownMove ()) 
				{
					//将已落下的方块稳定下来，重新生成另外一个方块
					for (int i = 0; i < _currentDownList.Count; i++) 
					{
                        if((int)_currentDownList [i].x >= 0)
                        {
                            _stableMatrix [(int)_currentDownList [i].x, (int)_currentDownList [i].y] = true;
                        }
					}

					//重置参数
					_fallDownTimeAccumulation = 0f;
                    _currentShape = null;

					//消除填满的格子
					//检查已经填满的行
					_fullRowIndexList.Clear ();
					for (int i = MAP_ROW_COUNT - 1; i >= 0; i--) 
					{
						bool isFull = true;
						for(int j = 0; j < MAP_COL_COUNT; j++)
						{
							if(!_stableMatrix[i, j])
							{
								isFull = false;
								break;								
							}
						}
						if (isFull) 
						{
							//消除满行
							for (int j = 0; j < MAP_COL_COUNT; j++) 
							{
								_stableMatrix [i, j] = false;
//                                Sequence sequence = DOTween.Sequence();
//                                sequence.AppendCallback(()=>{
//                                    GetSquareByIndex (i, j).transform.DOScale(0, 1.3f);
//                                });
//                                sequence.AppendInterval(1.3f);
//                                sequence.AppendCallback(()=>{
//                                    GetSquareByIndex (i, j).transform.localScale = Vector3.one;
//                                });
//                                sequence.Play();
                                GetSquareByIndex (i, j).SetActive (false);

							}
							_fullRowIndexList.Add (i);	
						}
					}

					//如果有满行，执行清除动作
					if(_fullRowIndexList.Count > 0)
					{						
						//找到当前棋盘的山顶
						int summit;
						for (summit = 0; summit < MAP_ROW_COUNT; summit++) 
						{
							bool isSummit = false;
							for (int j = 0; j < MAP_COL_COUNT; j++) 
							{
								if (_stableMatrix [summit, j]) 
								{
									isSummit = true;
									break;
								}
							}
							if (isSummit) 
							{
								break;
							}
						}

						int fullRowIndex = 0;
						for (int row = MAP_ROW_COUNT - 1; row >= summit; row--) 
						{
							while(fullRowIndex < _fullRowIndexList.Count && row == _fullRowIndexList[fullRowIndex])
							{
								row--;
								fullRowIndex++;
							}
							if (fullRowIndex > 0) 
							{
								for(int col = 0; col < MAP_COL_COUNT; col++)
								{
									_stableMatrix[row + fullRowIndex, col] = _stableMatrix[row, col];
									GetSquareByIndex (row + fullRowIndex, col).SetActive (_stableMatrix [row + fullRowIndex, col]);
                                    if(_stableMatrix[row, col])
                                    {
                                        GetSquareByIndex(row + fullRowIndex, col).GetComponent<Renderer>().material.color = GetSquareByIndex(row, col).GetComponent<Renderer>().material.color;
                                    }
									//将当前行清空
									_stableMatrix[row, col] = false;
									GetSquareByIndex (row, col).SetActive (false);
								}	
							}
						}
					}
				}
				else 
				{					
					for(int i = 0; i < _currentDownList.Count; i++)
					{
						//隐藏之前的位置
                        if((int)_currentDownList[i].x >= 0)
                        {
                            GetSquareByIndex ((int)_currentDownList[i].x, (int)_currentDownList[i].y).SetActive (false);                            
                        }
						//下落一格子
						_currentDownList [i] = new Vector2 (_currentDownList[i].x + 1, _currentDownList[i].y);
					}
                    _currentShapeOrigin += new Vector2(1, 0);

					//重新显示下降的方块
					for(int i = 0; i < _currentDownList.Count; i++)
					{
                        if((int)_currentDownList[i].x >= 0)
                        {
                            GetSquareByIndex ((int)_currentDownList[i].x, (int)_currentDownList[i].y).SetActive (true);
                            GetSquareByIndex((int)_currentDownList[i].x, (int)_currentDownList[i].y).GetComponent<Renderer>().material.color = _currentDownColor;
                        }
                    }
				}
			}
		}

		//玩家输入
		//向左移动
		if(Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
		{
			MoveLeft ();
			_leftTimeAccumulation = 0f;
		}
		if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
		{
			_leftTimeAccumulation += Time.deltaTime;
			if(_leftTimeAccumulation > _horizonTimeLimit)
			{
				_leftTimeAccumulation -= _horizonTimeLimit;
				MoveLeft ();
			}
		}

		//向右移动
		if(Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
		{
			MoveRight ();
			_rightTimeAccumulation = 0f;
		}
		if(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
		{
			_rightTimeAccumulation += Time.deltaTime;
			if(_rightTimeAccumulation > _horizonTimeLimit)
			{
				_rightTimeAccumulation -= _horizonTimeLimit;
				MoveRight ();
			}
		}

		//加速下降
		if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
		{
			//找到最后的位置，直接落下
			int downCount = 0;
			while(CheckDownMove(downCount))
			{
				downCount++;
			}
			downCount--;

			if(downCount > 0)
			{
				for (int i = 0; i < _currentDownList.Count; i++) 
				{
                    if((int)_currentDownList [i].x >= 0)
                    {
                        GetSquareByIndex ((int)_currentDownList [i].x, (int)_currentDownList [i].y).SetActive (false);
                    }
					_currentDownList [i] = new Vector2 (_currentDownList [i].x + downCount, _currentDownList [i].y);
				}
				//重新显示方块
				for(int i = 0; i < _currentDownList.Count; i++)
				{
                    if((int)_currentDownList[i].x >= 0)
                    {
                        GetSquareByIndex ((int)_currentDownList[i].x, (int)_currentDownList[i].y).SetActive (true);
                        GetSquareByIndex((int)_currentDownList[i].x, (int)_currentDownList[i].y).GetComponent<Renderer>().material.color = _currentDownColor;
                    }
				}
                _currentShapeOrigin += new Vector2(downCount, 0);
				_fallDownTimeAccumulation = 0f;
			}
		}

		//变形
		if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
		{
			//按照一定形状将下落的方块变形，如果不能变形，则没有效果
//            _currentShape.GetSwiftShapeMatrix();
            List<Vector2> tempList = _currentShape.GetSwiftDownSquareListByOrigin(_currentShapeOrigin);
            bool isValidSwift = true;
            for(int i = 0; i < tempList.Count; i++)
            {
                if(!CheckValidByIndex((int)tempList[i].x, (int)tempList[i].y))
                {
                    isValidSwift = false;
                    break;
                }
            }
            if(isValidSwift)
            {
                //隐藏原来的
                for(int i = 0; i < _currentDownList.Count; i++)
                {
                    if((int)_currentDownList [i].x >= 0)
                    {
                        GetSquareByIndex ((int)_currentDownList [i].x, (int)_currentDownList [i].y).SetActive (false);
                    }    
                }

                _currentShape.Swift();
                _currentDownList = _currentShape.GetMapDownSquareListByOrigin(_currentShapeOrigin);
                //显示变形之后的下落方块
                for(int i = 0; i < _currentDownList.Count; i++)
                {
                    if((int)_currentDownList [i].x >= 0)
                    {
                        GetSquareByIndex ((int)_currentDownList [i].x, (int)_currentDownList [i].y).SetActive (true);
                        GetSquareByIndex((int)_currentDownList[i].x, (int)_currentDownList[i].y).GetComponent<Renderer>().material.color = _currentDownColor;
                    }
                }
            }
		}
	}

	private void MoveLeft()
	{
		if(CheckLeftMove())
		{
			for (int i = 0; i < _currentDownList.Count; i++) 
			{
                if((int)_currentDownList[i].x >= 0)
                {
                    GetSquareByIndex ((int)_currentDownList [i].x, (int)_currentDownList [i].y).SetActive (false);
                }
				_currentDownList [i] = new Vector2 (_currentDownList [i].x, _currentDownList [i].y - 1);
			}
			//重新显示方块
            for(int i = 0; i < _currentDownList.Count; i++)
            {
                if((int)_currentDownList[i].x >= 0)
                {
                    GetSquareByIndex((int)_currentDownList[i].x, (int)_currentDownList[i].y).SetActive(true);
                    GetSquareByIndex((int)_currentDownList[i].x, (int)_currentDownList[i].y).GetComponent<Renderer>().material.color = _currentDownColor;
                }
            }
            _currentShapeOrigin += new Vector2(0, -1);
		}
	}

	private void MoveRight()
	{
		if(CheckRightMove())
		{
			for (int i = 0; i < _currentDownList.Count; i++) 
			{
                if((int)_currentDownList[i].x >= 0)
                {
                    GetSquareByIndex ((int)_currentDownList [i].x, (int)_currentDownList [i].y).SetActive (false);
                }
				_currentDownList [i] = new Vector2 (_currentDownList [i].x, _currentDownList [i].y + 1);
			}
			//重新显示方块
			for(int i = 0; i < _currentDownList.Count; i++)
			{
                if((int)_currentDownList[i].x >= 0)
                {
                    GetSquareByIndex ((int)_currentDownList[i].x, (int)_currentDownList[i].y).SetActive (true);
                    GetSquareByIndex((int)_currentDownList[i].x, (int)_currentDownList[i].y).GetComponent<Renderer>().material.color = _currentDownColor;
                }
			}
            _currentShapeOrigin += new Vector2(0, 1);
		}
	}



	private bool CheckDownMove(int pDownCount = 1)
	{
		for(int i = 0; i < _currentDownList.Count; i++)
		{
			if (!CheckValidByIndex ((int)(_currentDownList [i].x + pDownCount), (int)(_currentDownList [i].y))) 
			{
				return false;
			}
		}
		return true;
	}


	private bool CheckLeftMove()
	{
		for (int i = 0; i < _currentDownList.Count; i++) 
		{
			if(!CheckValidByIndex((int)(_currentDownList[i].x), (int)(_currentDownList[i].y - 1)))
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckRightMove()
	{
		for (int i = 0; i < _currentDownList.Count; i++) 
		{
			if(!CheckValidByIndex((int)(_currentDownList[i].x), (int)(_currentDownList[i].y + 1)))
			{
				return false;
			}
		}
		return true;
	}


	private bool CheckValidByIndex(int pRow, int pColumn)
	{
		//检查是否在范围之内, 行数可以小于0
		if (pRow > MAP_ROW_COUNT - 1) 
		{
			return false;
		}
		if (pColumn < 0 || pColumn > MAP_COL_COUNT - 1) 
		{
			return false;
		}

		//检查是否已经被占用
        if(pRow > 0 && _stableMatrix[pRow, pColumn])
		{
			return false;
		}

		return true;
	}


	private GameObject GetSquareByIndex(int pRow, int pColumn)
	{
		return _squareList[pRow * MAP_COL_COUNT + pColumn];
	}

    private GameObject GetNextSquareByIndex(int pIndex, int pRow, int pColomn)
    {
        return _nextOnesSquareList[pIndex * 16 + pRow * 4 + pColomn];
    }

	//tools
	private Vector3 GetPositionByIndex(int pRow, int pColumn)
	{
		return _leftUpPosition + new Vector3(pColumn * UNIT_LENGTH, pRow * UNIT_LENGTH * -1f, 0f);
	}

    private Vector3 GetNextPositionByIndexes(int pIndex, int pRow, int pColumn)
    {
        return _nextLeftUpPosition 
            + new Vector3(pColumn * NEXT_UNIT_LENGTH, pRow * NEXT_UNIT_LENGTH * -1f, 0f)
            + new Vector3(0f, pIndex * -1f * NEXT_UNIT_GAP);
    }
}
