using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MapCreator : MonoBehaviour
{
    class Room : MonoBehaviour
    {
        public Vector2 originPoint;
        public Vector2 endPoint;

        public List<Vector2> accesiblePositions = new List<Vector2>();

        public Room(Vector2 originPoint_, Vector2 endPoint_)
        {
            originPoint = originPoint_;
            endPoint = endPoint_;
        }

        public Vector2 getDistanceVector()
        {
            return new Vector2(originPoint.x - endPoint.x, originPoint.y - endPoint.y);
        }
    }

    public int minRooms = 8;
    public int maxRooms = 10;
    public int wallSice = 1;
    public int randomizeSice = 4;
    public int oneBetween = 50;

    public GameObject wall;
    public GameObject floor;
    public GameObject enemy;

    int xSize, ySize, zSize;
    int[][][] map;
    int[][] dataMap;

    List<Room> rooms =  new List<Room>();

    GameObject mazeParent;
    GameObject enemysParent;

    void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        InicializeMap();
        MazeCreator();
        GenerateMap();
        PrintMap();
    }

    public void ResetGame()
    {
        rooms.Clear();
        DestroyAllChildren(mazeParent.transform);
        DestroyAllChildren(enemysParent.transform);

        StartGame();
    }

    public void DestroyAllChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }

    void InicializeMap()
    {
        xSize = 51; ySize = 51; zSize = 1;      

        map = new int[ySize][][];           //Inicio un mapa de tamaño y altura por x anchura y altura z
        dataMap =  new int[ySize][];

        for (int y = 0; y < ySize; y++)
        {
            map[y] = new int[xSize][];
            dataMap[y] = new int[xSize];

            for (int x = 0; x < xSize; x++)
            {
                map[y][x] = new int[zSize];
                map[y][x][0] = 1;
                //map[y][x][1] = 1;
            }
        }

        Debug.Log("Mapa inicializado correctamente.");
    }

    private void slicer(Vector2 min, Vector2 max)
    {
        int difX = (int)(max.x - min.x) / randomizeSice;
        int difY = (int)(max.y - min.y) / randomizeSice;

        int x = (int)UnityEngine.Random.Range(min.x + difX, max.x - difX);
        int y = (int)UnityEngine.Random.Range(min.y + difY, max.y - difY);

        int pos = 0;
        if (difX < difY)
            pos = 1;
        else if (difX == difY)
            pos = UnityEngine.Random.Range(0, 2);

        Vector2 originF;
        Vector2 endF;
        Vector2 originS;
        Vector2 endS;        

        if (pos == 0) {         // Corte Vertical
            originF = new Vector2(min.x + wallSice, min.y + wallSice);
            endF = new Vector2(x - wallSice, max.y - wallSice);

            originS = new Vector2(x + wallSice, min.y + wallSice);
            endS = new Vector2(max.x - wallSice, max.y - wallSice);
        }
        else         // Corte Horinzontal
        {
            originF = new Vector2(min.x + wallSice, min.y + wallSice);
            endF = new Vector2(max.x - wallSice, y - wallSice);

            originS = new Vector2(min.x + wallSice, y + wallSice);
            endS = new Vector2(max.x - wallSice, max.y - wallSice);
        }

        Room firstRoom = new Room(originF, endF);
        Room secondRoom = new Room(originS, endS);

        //Debug.Log(firstRoom.originPoint + " / " + firstRoom.endPoint);
        //Debug.Log(secondRoom.originPoint + " / " + secondRoom.endPoint + "\n");

        rooms.Add(firstRoom);
        rooms.Add(secondRoom);
    }

    private void MazeCreator()      //Edita los valores del mapa para crear un laverinto
    {
        int numberRooms = UnityEngine.Random.Range(minRooms, maxRooms);
        //int numberRooms = minRooms;
        Vector3 initPos = new Vector3(xSize / 2, 0, 0);
        Vector3 endPos = new Vector3(xSize / 2, ySize - 1, 0);

        Vector2 min = new Vector2(0, 0);
        Vector2 max = new Vector2(xSize - 1, ySize - 1);

        slicer(min, max);

        for (int i = 2; i < numberRooms; i++)
        {
            if (rooms.Count > 0)
            {
                //int randomdIndex = UnityEngine.Random.Range(0, rooms.Count);
                //Room randomRoom = rooms[randomdIndex];

                int indexRoom = 0;
                int actualIndex = 0;
                Room bigestRoom = rooms[0];
                
                foreach (Room room in rooms)
                {
                    if (indexRoom + 1 < rooms.Count)
                    {
                        float difX = (bigestRoom.endPoint.x - bigestRoom.originPoint.x) - (room.endPoint.x - room.originPoint.x);
                        float difY = (bigestRoom.endPoint.y - bigestRoom.originPoint.y) - (room.endPoint.y - room.originPoint.y);

                        if (difX <= 0 && difY <= 0) { bigestRoom = room; actualIndex = indexRoom; }                            
                        else if (difX + difY <= 0) { bigestRoom = room; actualIndex = indexRoom; }
                    }
                    indexRoom += 1;
                }

                //Debug.Log(actualIndex);

                rooms.RemoveAt(actualIndex);
                slicer(new Vector2(bigestRoom.originPoint.x - wallSice, bigestRoom.originPoint.y - wallSice),
                new Vector2(bigestRoom.endPoint.x + wallSice, bigestRoom.endPoint.y + wallSice));
            }
        }        

        Vector2 pos = new Vector2(xSize / 2, 0);

        for (int i = 0; i < rooms.Count; i++)
        {
            Room room = rooms[i];
            Vector2 goal = new Vector2((room.endPoint.x + room.originPoint.x) / 2, (room.endPoint.y + room.originPoint.y) / 2);

            //Debug.Log("Recorrido a: " + goal.x + " " + goal.y);

            //string recorrido = "";

            while (Vector2.Distance(pos, goal) > 0.25f)
            {
                for (int aI = 0; aI < 2; aI++)
                    for (int aJ = 0; aJ < 2; aJ++)
                        if ((int)pos.y + aI > -1 && (int)pos.y + aI < ySize && (int)pos.x + aJ > -1 && (int)pos.x + aJ < xSize)
                        {
                            map[(int)pos.y + aI][(int)pos.x + aJ][0] = 0;
                            dataMap[(int)pos.y + aI][(int)pos.x + aJ] = -1;
                        }

                //recorrido += pos.x + " " + pos.y + " / ";

                pos = Vector2.MoveTowards(pos, goal, 1);
            }
            //Debug.Log(recorrido);
        }

        Vector2 end = new Vector2(xSize / 2, ySize);

        while (Vector2.Distance(pos, end) > 0.15f)
        {
            for (int aI = 0; aI < 2; aI++)
                for (int aJ = 0; aJ < 2; aJ++)
                    if ((int)pos.y + aI > -1 && (int)pos.y + aI < ySize && (int)pos.x + aJ > -1 && (int)pos.x + aJ < xSize)
                    {
                        map[(int)pos.y + aI][(int)pos.x + aJ][0] = 0;
                        dataMap[(int)pos.y + aI][(int)pos.x + aJ] = -1;
                    }                        

            //recorrido += pos.x + " " + pos.y + " / ";

            pos = Vector2.MoveTowards(pos, end, 1);
        }

        int index = 0;
        foreach (Room room in rooms)
        {
            Vector2 center = new Vector2((room.originPoint.x + room.endPoint.x) / 2,
                                         (room.originPoint.y + room.endPoint.y) / 2);

            float radius = Mathf.Min((room.endPoint.x - room.originPoint.x) / 2,
                                     (room.endPoint.y - room.originPoint.y) / 2);

            for (int y = (int)room.originPoint.y; y <= (int)room.endPoint.y; y++)
            {
                for (int x = (int)room.originPoint.x; x <= (int)room.endPoint.x; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);

                    if (distance <= radius)  // Solo limpia dentro del radio
                    {
                        int enemyPercent = UnityEngine.Random.Range(0, oneBetween);

                        if (enemyPercent < 1)
                            map[y][x][0] = 2;

                        else
                            map[y][x][0] = 0;

                        room.accesiblePositions.Add(new Vector2(x, y));

                        dataMap[y][x] = index;
                    }
                }
            }
            index++;
        }
    }

    void PrintMap()         //Recorre el mapa para pintarlo por consola
    {
        string level = "";

        for (int z = 0; z < zSize; z++)
        {
            for (int y = 0; y < ySize; y++)
            {
                for (int x = 0; x < xSize; x++)
                {
                    //level += map[y][x][z].ToString();
                    level += GetIndexRoom(x,y);
                }
                level += "\n";
            }
            Debug.Log(level);
            level = "";
        }
        Debug.Log(rooms.Count);
    }

    private void GenerateMap()
    {
        if (mazeParent == null)
        {
            mazeParent = new GameObject("Maze"); // GameObject contenedor
            enemysParent = new GameObject("Enemys"); // GameObject contenedor
        }
        

        for (int y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                Vector3 position = new Vector3(y * wall.transform.localScale.x, 0, x * wall.transform.localScale.z); // Posiciona en el mundo

                GameObject obj;
                GameObject ene;

                if (map[y][x][0] == 1)
                {
                    obj = Instantiate(wall, position + new Vector3(0, -0.4f, 0), Quaternion.identity);
                    dataMap[y][x] = -2;
                }
                else
                {
                    obj = Instantiate(floor, position + new Vector3(0, -0.4f, 0), Quaternion.identity);

                    if (map[y][x][0] == 2)
                    {
                        ene = Instantiate(enemy, position + new Vector3(0, -0.05f, 0), Quaternion.identity);
                        ene.transform.parent = enemysParent.transform; // Asignar el padre
                    }
                }

                obj.transform.parent = mazeParent.transform; // Asignar el padre
            }
        }
    }

    public  int GetIndexRoom(int x, int y)
    {
        /*for (int i = 0; i < rooms.Count; i++) { 
            Room room = rooms[i];
            if (room.originPoint.x < x && room.originPoint.y < y
                && room.endPoint.x > x && room.endPoint.y > y && dataMap[y][x] != 1)
                return i;
        }
        return -1;*/

        return dataMap[y][x];
    }

    public Vector2 GetRandomPosInRoom(int index) { 
        int randomIndex = UnityEngine.Random.Range(0, rooms[index].accesiblePositions.Count);

        return rooms[index].accesiblePositions[randomIndex];
    }
}
