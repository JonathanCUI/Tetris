using System.Collections.Generic;
using UnityEngine;

namespace Tetris
{
	public enum BaseShapeType
	{
		I,
		J,
		L,
		O,
		S,
		Z,
		T
	}



	public class BaseShape
	{

//        _randomColorList.Add(Color.blue);
//        _randomColorList.Add(Color.yellow);
//        _randomColorList.Add(Color.cyan);
//        _randomColorList.Add(Color.magenta);
        private static List<Color> _colorList = new List<Color>{Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta};

		public BaseShape ()
		{
			_shapeMatrixList = new List<int[,]> ();
			_currentIndex = -1;
		}
        public Vector2 StartOffSet;
        public BaseShapeType Type;
        public Vector2 OriginPoint;
        public Color ShapeColor;

		private int _currentIndex;
		private List<int[,]> _shapeMatrixList;

        public void Swift()
        {
            _currentIndex++;
            if(_currentIndex >= _shapeMatrixList.Count)
            {
                _currentIndex = 0;
            }
        }

        public void ResetStartShapeAndColor()
        {
            _currentIndex = Random.Range (0, _shapeMatrixList.Count);
            ShapeColor = _colorList[Random.Range(0, _colorList.Count)];
        }

		public void AddShapeMatrix(int[,] pMatrix)
		{
			_shapeMatrixList.Add(pMatrix);
		}

        public List<Vector2> GetMapDownSquareListByOrigin(Vector2 pOriginPoint)
        {
            List<Vector2> tempList = new List<Vector2>();
            for(int i = 0; i < 4; i++)
            {
                for(int j = 0; j < 4; j++)
                {
                    if(_shapeMatrixList[_currentIndex][i, j] == 1)
                    {
                        tempList.Add(pOriginPoint + new Vector2(i, j));
                    }
                }
            }
            return tempList;
        }
        public List<Vector2> GetSwiftDownSquareListByOrigin(Vector2 pOriginPoint)
        {
            List<Vector2> tempList = new List<Vector2>();
            int tempIndex = (_currentIndex + 1 >= _shapeMatrixList.Count) ? 0 : _currentIndex + 1;
            for(int i = 0; i < 4; i++)
            {
                for(int j = 0; j < 4; j++)
                {
                    if(_shapeMatrixList[tempIndex][i, j] == 1)
                    {
                        tempList.Add(pOriginPoint + new Vector2(i, j));
                    }
                }
            }
            return tempList;
        }
	}

	public class ShapeManager
	{
		private ShapeManager()
		{
			_baseShapeList = new List<BaseShape> ();
			#region I
			BaseShape I = new BaseShape();
			I.Type = BaseShapeType.I;
            I.StartOffSet = new Vector2(-1, 3);
			I.AddShapeMatrix(
				new int[,]{
                {0, 0, 0, 0},
			    {1, 1, 1, 1},
                {0, 0, 0, 0},
                {0, 0, 0, 0}});
            I.AddShapeMatrix(
                new int[,]{
                {0, 1, 0, 0},
                {0, 1, 0, 0},
                {0, 1, 0, 0},
                {0, 1, 0, 0}});
            _baseShapeList.Add(I);
			#endregion
            #region J
            BaseShape J = new BaseShape();
            J.Type = BaseShapeType.J;
            J.StartOffSet = new Vector2(-1, 4);
            J.AddShapeMatrix(
                new int[,]{
                {1, 0, 0, 0},
                {1, 1, 1, 0},
                {0, 0, 0, 0},
                {0, 0, 0, 0}});
            J.AddShapeMatrix(
                new int[,]{
                {0, 1, 1, 0},
                {0, 1, 0, 0},
                {0, 1, 0, 0},
                {0, 0, 0, 0}});
            J.AddShapeMatrix(
                new int[,]{
                {0, 0, 0, 0},
                {1, 1, 1, 0},
                {0, 0, 1, 0},
                {0, 0, 0, 0}});
            J.AddShapeMatrix(
                new int[,]{
                {0, 1, 0, 0},
                {0, 1, 0, 0},
                {1, 1, 0, 0},
                {0, 0, 0, 0}});
            _baseShapeList.Add(J);
            #endregion
            #region L
            BaseShape L = new BaseShape();
            L.Type = BaseShapeType.L;
            L.StartOffSet = new Vector2(-1, 4);
            L.AddShapeMatrix(
                new int[,]{
                {0, 0, 1, 0},
                {1, 1, 1, 0},
                {0, 0, 0, 0},
                {0, 0, 0, 0}});
            L.AddShapeMatrix(
                new int[,]{
                {0, 1, 0, 0},
                {0, 1, 0, 0},
                {0, 1, 1, 0},
                {0, 0, 0, 0}});
            L.AddShapeMatrix(
                new int[,]{
                {0, 0, 0, 0},
                {1, 1, 1, 0},
                {1, 0, 0, 0},
                {0, 0, 0, 0}});
            L.AddShapeMatrix(
                new int[,]{
                {1, 1, 0, 0},
                {0, 1, 0, 0},
                {0, 1, 0, 0},
                {0, 0, 0, 0}});
            _baseShapeList.Add(L);
            #endregion
            #region O
            BaseShape O = new BaseShape();
            O.Type = BaseShapeType.O;
            O.StartOffSet = new Vector2(-1, 3);
            O.AddShapeMatrix(
                new int[,]{
                {0, 1, 1, 0},
                {0, 1, 1, 0},
                {0, 0, 0, 0},
                {0, 0, 0, 0}});
            _baseShapeList.Add(O);
            #endregion          
            #region S
            BaseShape S = new BaseShape();
            S.Type = BaseShapeType.S;
            S.StartOffSet = new Vector2(-1, 3);
            S.AddShapeMatrix(
                new int[,]{
                {0, 1, 1, 0},
                {1, 1, 0, 0},
                {0, 0, 0, 0},
                {0, 0, 0, 0}});
            S.AddShapeMatrix(
                new int[,]{
                {1, 0, 0, 0},
                {1, 1, 0, 0},
                {0, 1, 0, 0},
                {0, 0, 0, 0}});
            _baseShapeList.Add(S);
            #endregion
            #region Z
            BaseShape Z = new BaseShape();
            Z.Type = BaseShapeType.Z;
            Z.StartOffSet = new Vector2(-1, 3);
            Z.AddShapeMatrix(
                new int[,]{
                {1, 1, 0, 0},
                {0, 1, 1, 0},
                {0, 0, 0, 0},
                {0, 0, 0, 0}});
            Z.AddShapeMatrix(
                new int[,]{
                {0, 0, 1, 0},
                {0, 1, 1, 0},
                {0, 1, 0, 0},
                {0, 0, 0, 0}});
            _baseShapeList.Add(Z);
            #endregion
            #region T
            BaseShape T = new BaseShape();
            T.Type = BaseShapeType.T;
            T.StartOffSet = new Vector2(-1, 3);
            T.AddShapeMatrix(
                new int[,]{
                {0, 1, 0, 0},
                {1, 1, 1, 0},
                {0, 0, 0, 0},
                {0, 0, 0, 0}});
            T.AddShapeMatrix(
                new int[,]{
                {0, 1, 0, 0},
                {0, 1, 1, 0},
                {0, 1, 0, 0},
                {0, 0, 0, 0}});
            T.AddShapeMatrix(
                new int[,]{
                {0, 0, 0, 0},
                {1, 1, 1, 0},
                {0, 1, 0, 0},
                {0, 0, 0, 0}});
            T.AddShapeMatrix(
                new int[,]{
                {0, 1, 0, 0},
                {1, 1, 0, 0},
                {0, 1, 0, 0},
                {0, 0, 0, 0}});
            _baseShapeList.Add(T);
            #endregion
		}

		public static ShapeManager Instance{
			get
            {
				return _instance;
			}
		}


		public static void Initialize()
		{
			_instance = new ShapeManager();
		}

		//data members
		private static ShapeManager _instance;

		//存放7种基本形状
		private List<BaseShape> _baseShapeList;

        public BaseShape GetNextShape()
        {
            return _baseShapeList[Random.Range(0, _baseShapeList.Count)];
        }
        //最后还是用来敲代码，合适用不同的机械键盘

	}
}

