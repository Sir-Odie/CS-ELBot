// Eternal Lands Bot
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace cs_elbot.AdvancedCommunication
{
    /// <summary>
    /// Description of help_command_handler.
    /// </summary>
    public class GotoCommandHandler
    {
        public int difference(int thisPoint, int thatPoint)
        {
            int theDifference = 0;
            if (thisPoint > thatPoint)
            {
                theDifference = thisPoint - thatPoint;
            }
            else
            {
                theDifference = thatPoint - thisPoint;
            }
            return theDifference;
        }
        public class pathing
        {
            //constants
            public const int MAXATTEMPTS = 200000;
            private string myMapName = "";
            public enum myStates
            {
                STATE_NONE = 0, PF_STATE_OPEN, PF_STATE_CLOSED
            }

            public struct object3D
            {
                public char[] objectFileName; //this will be 80 characters
                public float x_pos;
                public float y_pos;
                public float z_pos;

                public float x_rot;
                public float y_rot;
                public float z_rot;

                public byte self_lit;
                public byte blended;

                public byte[] padding;  //this will be 2 characters

                public float r;
                public float g;
                public float b;

                public byte[] reserved; //24 characters for later use
                public int object_id;
            }

            private struct mapHeader
            {
                //put in a catch for not finding the file though that'd be pretty ugly...
                public char[] file_sig;//should be "elmf", or else the map is invalid
                public int tile_map_x_len;
                public int tile_map_y_len;
                public int tile_map_offset;
                public int height_map_offset;
                public int obj_3d_struct_len;
                public int obj_3d_no;
                public int obj_3d_offset;
                public int obj_2d_struct_len;
                public int obj_2d_no;
                public int obj_2d_offset;
                public int lights_struct_len;
                public int lights_no;
                public int lights_offset;
                public char dungeon;//no sun
                public char res_2;
                public char res_3;
                public char res_4;
                public float ambient_r;
                public float ambient_g;
                public float ambient_b;
                public int particles_struct_len;
                public int particles_no;
                public int particles_offset;
                public int reserved_8;
                public int reserved_9;
                public int reserved_10;
                public int reserved_11;
                public int reserved_12;
                public int reserved_13;
                public int reserved_14;
                public int reserved_15;
                public int reserved_16;
                public int reserved_17;
            }
            private BinaryReader myReader;
            private mapHeader myMapHeader = new mapHeader();
            public struct TILE
            {
                public int open_pos;
                public int x;
                public int y;
                public int f;
                public int g;
                public int z;
                public myStates state;

                public int parentX;
                public int parentY;
            }
            private TILE[] OPEN_LIST;
            private object3D[] my3DObjects;
            public static int OPEN_LISTCount;
            private TILE[] TILE_MAP;
            public static int TILE_MAPCount;
            private TILE[] myPathTiles;
            public static int pathCount;
            //public static int moveCount = 0;
            private ArrayList myPath = new ArrayList();


            private TILE destTile = new TILE();
            private TILE srcTile = new TILE();

            private byte[] tileMap;
            private char[] heightMap;

            //mapping methods
            private void readMapHeader()
            {
                bool localDebug = false;
                myMapHeader.file_sig = myReader.ReadChars(4);
                myMapHeader.tile_map_x_len = myReader.ReadInt32();
                myMapHeader.tile_map_y_len = myReader.ReadInt32();
                myMapHeader.tile_map_offset = myReader.ReadInt32();
                myMapHeader.height_map_offset = myReader.ReadInt32();
                myMapHeader.obj_3d_struct_len = myReader.ReadInt32();
                myMapHeader.obj_3d_no = myReader.ReadInt32();
                myMapHeader.obj_3d_offset = myReader.ReadInt32();
                myMapHeader.obj_2d_struct_len = myReader.ReadInt32();
                myMapHeader.obj_2d_no = myReader.ReadInt32();
                myMapHeader.obj_2d_offset = myReader.ReadInt32();
                myMapHeader.lights_struct_len = myReader.ReadInt32();
                myMapHeader.lights_no = myReader.ReadInt32();
                myMapHeader.lights_offset = myReader.ReadInt32();
                myMapHeader.dungeon = myReader.ReadChar();
                myMapHeader.res_2 = myReader.ReadChar();
                myMapHeader.res_3 = myReader.ReadChar();
                myMapHeader.res_4 = myReader.ReadChar();
                myMapHeader.ambient_r = myReader.ReadSingle();
                myMapHeader.ambient_g = myReader.ReadSingle();
                myMapHeader.ambient_b = myReader.ReadSingle();
                myMapHeader.particles_struct_len = myReader.ReadInt32();
                myMapHeader.particles_no = myReader.ReadInt32();
                myMapHeader.particles_offset = myReader.ReadInt32();
                myMapHeader.reserved_8 = myReader.ReadInt32();
                myMapHeader.reserved_9 = myReader.ReadInt32();
                myMapHeader.reserved_10 = myReader.ReadInt32();
                myMapHeader.reserved_11 = myReader.ReadInt32();
                myMapHeader.reserved_12 = myReader.ReadInt32();
                myMapHeader.reserved_13 = myReader.ReadInt32();
                myMapHeader.reserved_14 = myReader.ReadInt32();
                myMapHeader.reserved_15 = myReader.ReadInt32();
                myMapHeader.reserved_16 = myReader.ReadInt32();
                myMapHeader.reserved_17 = myReader.ReadInt32();
                //write out the information for debugging purposes
                if (localDebug == true)
                {
                    Console.Write("file_sig :");
                    Console.WriteLine(myMapHeader.file_sig);
                    Console.WriteLine("tile_map_x_len :" + myMapHeader.tile_map_x_len);
                    Console.WriteLine("tile_map_y_len :" + myMapHeader.tile_map_y_len);
                    Console.WriteLine("tile_map_offset :" + myMapHeader.tile_map_offset);
                    Console.WriteLine("height_map_offset :" + myMapHeader.height_map_offset);
                    Console.WriteLine("obj_3d_struct_len :" + myMapHeader.obj_3d_struct_len);
                    Console.WriteLine("obj_3d_no :" + myMapHeader.obj_3d_no);
                    Console.WriteLine("obj_3d_offset :" + myMapHeader.obj_3d_offset);
                    Console.WriteLine("obj_2d_struct_len :" + myMapHeader.obj_2d_struct_len);
                    Console.WriteLine("obj_2d_no :" + myMapHeader.obj_2d_no);
                    Console.WriteLine("obj_2d_offset :" + myMapHeader.obj_2d_offset);
                    Console.WriteLine("lights_struct_len :" + myMapHeader.lights_struct_len);
                    Console.WriteLine("lights_no :" + myMapHeader.lights_no);
                    Console.WriteLine("lights_offset :" + myMapHeader.lights_offset);
                    Console.WriteLine("dungeon :" + myMapHeader.dungeon);
                    Console.WriteLine("res_2 :" + myMapHeader.res_2);
                    Console.WriteLine("res_3 :" + myMapHeader.res_3);
                    Console.WriteLine("res_4 :" + myMapHeader.res_3);
                    Console.WriteLine("ambient_r :" + myMapHeader.ambient_r);
                    Console.WriteLine("ambient_g :" + myMapHeader.ambient_g);
                    Console.WriteLine("ambient_b :" + myMapHeader.ambient_b);
                    Console.WriteLine("particles_struct_len :" + myMapHeader.particles_struct_len);
                    Console.WriteLine("particles_no :" + myMapHeader.particles_no);
                    Console.WriteLine("particles_offset :" + myMapHeader.particles_offset);
                    Console.WriteLine("reserved_8 :" + myMapHeader.reserved_8);
                    Console.WriteLine("reserved_9 :" + myMapHeader.reserved_9);
                    Console.WriteLine("reserved_10 :" + myMapHeader.reserved_10);
                    Console.WriteLine("reserved_11 :" + myMapHeader.reserved_11);
                    Console.WriteLine("reserved_12 :" + myMapHeader.reserved_12);
                    Console.WriteLine("reserved_13 :" + myMapHeader.reserved_13);
                    Console.WriteLine("reserved_14 :" + myMapHeader.reserved_14);
                    Console.WriteLine("reserved_15 :" + myMapHeader.reserved_15);
                    Console.WriteLine("reserved_16 :" + myMapHeader.reserved_16);
                    Console.WriteLine("reserved_17 :" + myMapHeader.reserved_17);
                    Console.WriteLine("Press enter to continue.");
                    Console.ReadLine();
                }
            }
            private void readTileMap()
            {
                //not using this, just reading the tile map to get the file pointer to the right spot
                //might be able to just replace this with a call to move the file pointer this many bytes and save some memory...
                bool localdebug = false;
                if (localdebug == true)
                {
                    Console.WriteLine("tile map read..." + myMapHeader.tile_map_x_len * myMapHeader.tile_map_y_len);
                    Console.ReadLine();
                }
                tileMap = myReader.ReadBytes(myMapHeader.tile_map_x_len * myMapHeader.tile_map_y_len);
            }
            private void readHeightMap()
            {
                bool localdebug = false;
                //                int tileCount = 0;
                int i;
                heightMap = myReader.ReadChars(myMapHeader.tile_map_x_len * myMapHeader.tile_map_y_len * 6 * 6);
                if (localdebug)
                {
                    Console.WriteLine("height map read..." + heightMap.Length);
                    Console.ReadLine();
                }

                //insert the heights into mymap array
                for (int x = 0; x < myMapHeader.tile_map_x_len * 6; x++)
                {
                    for (int y = 0; y < myMapHeader.tile_map_y_len * 6; y++)
                    {
                        i = y * myMapHeader.tile_map_x_len * 6 + x;
                        TILE myTile = new TILE();
                        myTile.x = x;
                        myTile.y = y;
                        myTile.z = heightMap[i];
                        myTile.state = myStates.STATE_NONE;
                        myTile.parentX = -1;
                        myTile.parentY = -1;
                        TILE_MAP[i] = myTile;
                        TILE_MAPCount++;
                    }
                }

                if (localdebug)
                {
                    Console.WriteLine(TILE_MAPCount);
                    Console.ReadLine();
                }
            }
            public void printMap()
            {
                bool localdebug = false;
                Console.WindowWidth = (myMapHeader.tile_map_x_len * 6) + 1;
                int counter = 0;
                //                ArrayList myMap = TILE_MAP;
                TILE[] myMap = TILE_MAP;
                foreach (TILE myTile in myMap)
                {
                    counter++;
                    if (localdebug)
                    {
                        Console.WriteLine(myTile.x + "," + myTile.y);
                        Console.ReadLine();
                    }
                    else
                    {
                        Console.SetCursorPosition(myTile.x, Console.CursorTop);
                        if (myTile.z != 0)
                        {
                            Console.Write('1');
                        }
                        else
                        {
                            Console.Write('X');
                        }
                        if ((counter % (myMapHeader.tile_map_x_len * 6)) == 0)
                        {
                            Console.WriteLine();
                        }
                    }
                }
            }
            public void resetMap()
            {
                bool debug = false;
                if (debug)
                {
                    Console.WriteLine("Resetting map!");
                    Console.WriteLine(MainClass.mapName);
                }
                if (MainClass.mapName != "")
                {
                    string tempMapName = MainClass.launchPath.Replace('/', '\\') + MainClass.mapName.Replace('/', '\\');
                    {
                        myMapName = tempMapName;
                        if (File.Exists(myMapName))
                        {
                            if (debug)
                            {
                                Console.WriteLine("File {0} exists!", myMapName);
                            }
                            try
                            {
                                myReader = new BinaryReader(File.Open(myMapName, FileMode.Open));
                                readMapHeader();
                                readTileMap();
                                OPEN_LIST = new TILE[myMapHeader.tile_map_x_len * myMapHeader.tile_map_y_len * 6 * 6];
                                OPEN_LISTCount = 0;
                                TILE_MAP = new TILE[myMapHeader.tile_map_x_len * myMapHeader.tile_map_y_len * 6 * 6];
                                myPathTiles = new TILE[MAXATTEMPTS];
                                readHeightMap();
                                read3DMapObjects(myReader);
                                myReader.Close();
                                myPath = new ArrayList();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Exception found: " + e);
                                Console.WriteLine("error opening file...");
                                Console.WriteLine("name of map = " + myMapName);
                            }
                        }
                        else
                        {
                            Console.WriteLine("File {0} not found!", myMapName);
                        }
                    }
                }
            }
            public void clearMap()
            {
                myMapHeader = new mapHeader();
                tileMap = new byte[0];
                OPEN_LIST = new TILE[0];
                OPEN_LISTCount = 0;
                TILE_MAP = new TILE[0];
                myPathTiles = new TILE[0];
                heightMap = new char[0];
                my3DObjects = new object3D[0];
                myPath = new ArrayList();
                GC.Collect();
            }
            public ArrayList listObjects(string searchWord)
            {
                ArrayList matchingObjects = new ArrayList();
                foreach (object3D my3DObject in my3DObjects)
                {
                    string filename = new string(my3DObject.objectFileName);
                    //                    string filename = BitConverter.ToString( my3DObject.objectFileName );
                    if (filename.Contains(searchWord))
                    {
                        matchingObjects.Add(my3DObject);
                    }
                }
                return matchingObjects;
            }
            public ArrayList listObjects(ActorHandler.position currentLocation)
            {
                ArrayList matchingObjects = new ArrayList();
                foreach (object3D my3DObject in my3DObjects)
                {
                    if (difference((int)my3DObject.x_pos, currentLocation.x) < 12 && difference((int)my3DObject.y_pos, currentLocation.y) < 12)
                    {
                        //make it skip ground objects in this function
                        string filename = new string(my3DObject.objectFileName);
                        if (!filename.Contains("ground"))
                        {
                            matchingObjects.Add(my3DObject);
                        }
                    }
                }
                return matchingObjects;
            }
            public void read3DMapObjects(BinaryReader myReader)
            {
                my3DObjects = new object3D[myMapHeader.obj_3d_no];
                for (int count = 0; count < myMapHeader.obj_3d_no; count++)
                {
                    object3D my3DObject = new object3D();
                    my3DObject.objectFileName = myReader.ReadChars(80);
                    my3DObject.x_pos = myReader.ReadSingle() * 2;
                    my3DObject.y_pos = myReader.ReadSingle() * 2;
                    my3DObject.z_pos = myReader.ReadSingle() * 2;
                    my3DObject.x_rot = myReader.ReadSingle() * 2;
                    my3DObject.y_rot = myReader.ReadSingle() * 2;
                    my3DObject.z_rot = myReader.ReadSingle() * 2;
                    my3DObject.self_lit = myReader.ReadByte();
                    my3DObject.blended = myReader.ReadByte();
                    my3DObject.padding = myReader.ReadBytes(2);
                    my3DObject.r = myReader.ReadSingle() * 2;
                    my3DObject.g = myReader.ReadSingle() * 2;
                    my3DObject.b = myReader.ReadSingle() * 2;
                    my3DObject.reserved = myReader.ReadBytes(24);
                    my3DObject.object_id = count;
                    my3DObjects[count] = my3DObject;
                }
            }

            //pathing methods
            public TILE getTile(int x, int y)
            {
                TILE myTile = new TILE();
                if (x >= myMapHeader.tile_map_x_len * 6 || y >= myMapHeader.tile_map_y_len * 6 || x < 0 || y < 0)
                {
                    //should probably tell the user about this...
                }
                else
                {
                    myTile = (TILE)TILE_MAP[y * myMapHeader.tile_map_x_len * 6 + x];
                }
                return myTile;
            }
            public static int difference(int thisPoint, int thatPoint)
            {
                int theDifference = 0;
                if (thisPoint > thatPoint)
                {
                    theDifference = thisPoint - thatPoint;
                }
                else
                {
                    theDifference = thatPoint - thisPoint;
                }
                return theDifference;
            }
            private bool areDiagonal(TILE thisGridMark, TILE thatGridMark)
            {
                bool theyAreDiagonal = true;
                if ((thatGridMark.x == thisGridMark.x) || (thatGridMark.y == thisGridMark.y))
                {
                    theyAreDiagonal = false;
                }
                return theyAreDiagonal;
            }
            private int distance(TILE thisGridMark, TILE thatGridMark)
            {
                //this is the heuristic function used to calculate distance
                int theDistance = 0;
                //PF_HEUR(a, b) ((PF_DIFF(a->x, b->x) + PF_DIFF(a->y, b->y)) * 10) //stolen from client code (same in redknight afaik...)
                theDistance = (difference(thisGridMark.x, thatGridMark.x) + difference(thisGridMark.y, thatGridMark.y)) * 10;
                return theDistance;
            }
            private class mySorter : IComparer
            {
                int IComparer.Compare(object x, object y)
                {
                    TILE myTile = (TILE)x;
                    TILE myOtherTile = (TILE)y;
                    return myTile.f.CompareTo(myOtherTile.f);
                }
            }
            private void addToOpenList(TILE current, TILE neighbor)
            {
                TILE nullTile = new TILE();
                TILE tempTile = new TILE();
                bool localdebug = false;
                if (localdebug)
                {
                    printOpenTileList();
                }
                if (neighbor.Equals(nullTile) || neighbor.z == 0 || (!current.Equals(nullTile) && difference(current.z, neighbor.z) > 2))
                {
                    return;
                }

                if (!current.Equals(nullTile))
                {

                    int myG = 0, myF = 0, myH = 0;
                    bool diagonal = (neighbor.x != current.x && neighbor.y != current.y);
                    myG = current.g + (diagonal ? 14 : 10);
                    myH = distance(neighbor, destTile);
                    myF = myG + myH;
                    if (neighbor.state != myStates.STATE_NONE && myF >= neighbor.f)
                    {
                        return;
                    }
                    neighbor.f = myF;
                    neighbor.g = myG;
                    neighbor.parentX = current.x;
                    neighbor.parentY = current.y;
                }
                else
                {
                    neighbor.f = distance(srcTile, destTile);
                    neighbor.g = 0;
                    //can't just assign NULL here like the client is, and can't have pointers, so using -1,-1 to represent the NULL TILE
                    neighbor.parentX = -1;
                    neighbor.parentY = -1;
                }

                if (neighbor.state != myStates.PF_STATE_OPEN)
                {
                    //adding state open here...
                    neighbor.open_pos = ++OPEN_LISTCount;
                    //neighbor.open_pos = OPEN_LIST.Count + 1;
                    OPEN_LIST[neighbor.open_pos] = neighbor;
                }
                if (localdebug)
                {
                    printOpenTileList();
                }
                //sort them by their F value...
                while (neighbor.open_pos > 1)
                {
                    //need to be updating the open_pos it appears...
                    if (OPEN_LIST[neighbor.open_pos].f <= OPEN_LIST[neighbor.open_pos / 2].f)
                    {
                        tempTile = OPEN_LIST[neighbor.open_pos / 2];
                        OPEN_LIST[neighbor.open_pos / 2] = OPEN_LIST[neighbor.open_pos];
                        OPEN_LIST[neighbor.open_pos] = tempTile;
                        neighbor.open_pos /= 2;
                    }
                    else
                    {
                        break;
                    }
                }
                TILE myTempTIle = TILE_MAP[neighbor.y * myMapHeader.tile_map_x_len * 6 + neighbor.x];
                myTempTIle.state = myStates.PF_STATE_OPEN;
                TILE_MAP[neighbor.y * myMapHeader.tile_map_x_len * 6 + neighbor.x] = myTempTIle;
            }
            private void printOpenTileList()
            {
                for (int count = 1; count < OPEN_LISTCount; count++)
                {
                    TILE myTile = OPEN_LIST[count];
                    printTile(myTile);
                    if (count % 50 == 0)
                    {
                        Console.ReadLine();
                    }
                }
                Console.WriteLine("------------------------------------------");
                Console.ReadLine();
            }
            public void printTile(TILE myTile)
            {
                Console.WriteLine(myTile.x + "," + myTile.y + "," + myTile.f + "," + myTile.g + "," + myTile.parentX + "," + myTile.parentY);
            }
            public void finishPath()
            {
                bool localdebug = false;
                int moveCount = 0;
                TILE parentTIle = myPathTiles[pathCount - 1];
                for (int count = pathCount - 1; count > 0; count--)
                {
                    TILE myTile = myPathTiles[count];
                    if (myTile.x == parentTIle.parentX && myTile.y == parentTIle.parentY)
                    {
                        moveCount++;
                        myPath.Add(myTile);
                        parentTIle = myTile;
                    }
                }
                myPath.Reverse();
                if (localdebug)
                {
                    Console.WriteLine("path finished: " + moveCount);
                }
            }

            public void printPath(ArrayList myPrintPath)
            {
                FileStream fs = new FileStream("out.txt", FileMode.Create);
                TextWriter myTextWriter = Console.Out;
                StreamWriter sw = new StreamWriter(fs);
                int pathTiles = 0;
                foreach (TILE myTile in myPrintPath)
                {
                    //Console.SetOut(sw);
                    printTile(myTile);
                    pathTiles++;
                    if (pathTiles % 100 == 0)
                    {
                        Console.ReadLine();
                    }
                }
                //Console.SetOut(myTextWriter);
                //sw.Close();
                Console.WriteLine("Path size: " + pathTiles);
            }
            private TILE getNextOpenTile()
            {
                bool localdebug = false;
                if (localdebug)
                {
                    //                    Console.WriteLine("before");
                    //                    printOpenTileList();
                }
                //this will probably look ugly at first, the client code is very hard to understand here
                //so, going to copy it's logic as closely as possible, ignoring the fact that I haven't a clue what it's doing, lol
                TILE retTile = new TILE();
                TILE tempTile = new TILE();
                int i = 0, j = 0;
                bool done = false;
                if (OPEN_LISTCount == 0)
                {
                    return new TILE();
                }
                retTile = OPEN_LIST[1];

                //swapping around tiles for some reason... heh
                retTile.state = myStates.PF_STATE_CLOSED;
                TILE myTempTile = TILE_MAP[retTile.y * myMapHeader.tile_map_x_len * 6 + retTile.x];
                myTempTile.state = myStates.PF_STATE_CLOSED;
                TILE_MAP[retTile.y * myMapHeader.tile_map_x_len * 6 + retTile.x] = myTempTile;
                //this isn't working, let's see what we can replace it with
                OPEN_LIST[1] = OPEN_LIST[OPEN_LISTCount--];

                j = 1;
                while (!done)
                {
                    i = j;
                    if (2 * i + 1 <= OPEN_LISTCount)
                    {
                        if (OPEN_LIST[i].f >= OPEN_LIST[2 * i].f)
                        {
                            j = 2 * i;
                        }
                        if (OPEN_LIST[j].f >= OPEN_LIST[2 * i + 1].f)
                        {
                            j = 2 * i + 1;
                        }
                    }
                    else if (2 * i <= OPEN_LISTCount)
                    {
                        if (OPEN_LIST[i].f >= OPEN_LIST[2 * i].f)
                        {
                            j = 2 * i;
                        }
                    }
                    if (i != j)
                    {
                        tempTile = OPEN_LIST[i];
                        OPEN_LIST[i] = OPEN_LIST[j];
                        OPEN_LIST[j] = tempTile;
                    }
                    else
                    {
                        done = true;
                    }
                }
                if (localdebug)
                {
                    //                    Console.WriteLine("after");
                    //                    printOpenTileList();
                    Console.Write("Choosing tile: ");
                    printTile(retTile);
                    Console.ReadLine();
                }
                return retTile;
            }
            public ArrayList getPath(int fromX, int fromY, int toX, int toY)
            {
                bool localdebug = true;
                //don't forget to marry this up with the actors at some point :P
                //
                srcTile = getTile(fromX, fromY);
                if (localdebug)
                {
                    printTile(srcTile);
                }
                if (srcTile.z == 0)
                {
                    //this shouldn't happen after it's automated, only with bad user input...
                    Console.WriteLine("Bad source tile, returning with no path...");
                    return myPath;
                }
                destTile = getTile(toX, toY);
                if (localdebug)
                {
                    printTile(destTile);
                }
                if (destTile.z == 0)
                {
                    //this means we have an object in the way (unless the coords are really bad, either way looking around for a good square can't hurt)
                    //see if we can find a sqaure adjacent to this one that's open...
                    TILE tempDest = getTile(toX, toY + 1);
                    if (destTile.z == 0)
                    {
                        tempDest = getTile(toX, toY - 1);
                    }
                    if (destTile.z == 0)
                    {
                        tempDest = getTile(toX + 1, toY);
                    }
                    if (destTile.z == 0)
                    {
                        tempDest = getTile(toX - 1, toY);
                    }
                    if (destTile.z == 0)
                    {
                        tempDest = getTile(toX + 1, toY + 1);
                    }
                    if (destTile.z == 0)
                    {
                        tempDest = getTile(toX + 1, toY - 1);
                    }
                    if (destTile.z == 0)
                    {
                        tempDest = getTile(toX - 1, toY + 1);
                    }
                    if (destTile.z == 0)
                    {
                        tempDest = getTile(toX - 1, toY - 1);
                    }
                    if (destTile.z == 0)
                    {
                        Console.WriteLine("Bad destination tile, returning with no path...");
                        return myPath;
                    }
                }
                for (int gridCount = 0; gridCount < myMapHeader.tile_map_x_len * myMapHeader.tile_map_y_len * 6 * 6; gridCount++)
                {
                    TILE myTile = TILE_MAP[gridCount];
                    myTile.state = myStates.STATE_NONE;
                    myTile.parentX = -1;
                    myTile.parentY = -1;
                    TILE_MAP[gridCount] = myTile;
                }
                pathCount = 0;
                addToOpenList(new TILE(), srcTile);
                bool pathFound = false;
                while (pathCount < MAXATTEMPTS)
                {
                    TILE myTile = getNextOpenTile();
                    if (myTile.x == destTile.x && myTile.y == destTile.y)
                    {
                        pathFound = true;
                        //Console.WriteLine("YAY, we found it!");
                        //Console.ReadLine();
                        break;
                    }
                    myPathTiles[pathCount] = myTile;
                    addToOpenList(myTile, getTile(myTile.x, myTile.y + 1));
                    addToOpenList(myTile, getTile(myTile.x + 1, myTile.y + 1));
                    addToOpenList(myTile, getTile(myTile.x + 1, myTile.y));
                    addToOpenList(myTile, getTile(myTile.x + 1, myTile.y - 1));
                    addToOpenList(myTile, getTile(myTile.x, myTile.y - 1));
                    addToOpenList(myTile, getTile(myTile.x - 1, myTile.y - 1));
                    addToOpenList(myTile, getTile(myTile.x - 1, myTile.y));
                    addToOpenList(myTile, getTile(myTile.x - 1, myTile.y + 1));
                    pathCount++;
                }
                if (!pathFound  || pathCount == 0)
                {
                    myPath = new ArrayList();
                    Console.WriteLine("no path returned! :P");
                }
                else
                {
                    Console.WriteLine("path returned! :P" + "|" + pathCount);
                    finishPath();
                }
                Console.WriteLine("finshed path count:" + myPath.Count);
                return myPath;
            }

            public pathing(string mapName)
            {
            }
        }
        public int movecount = 0;
        private string username = "";
        private TCPWrapper TheTCPWrapper;
        private BasicCommunication.MessageParser TheMessageParser;
        private AdminHelpCommandHandler TheAdminHelpCommandHandler;
        private MySqlManager TheMySqlManager;
        private PMHandler ThePMHandler;
        private ActorHandler TheActorHandler;
        private System.Collections.ArrayList CommandArrayList = new System.Collections.ArrayList();
        private pathing myPathing = new pathing("C:\\bot\\Test\\" + MainClass.mapName);
        //callback timer
        public bool moveDone = false;
        private bool keepMoving = false;
        //movement
        public ArrayList myMoves = new ArrayList();
        public int myMovesCount = 0;
        public int startX = 0;
        public int startY = 0;
        public int destX = 0;
        public int destY = 0;
        public int previousX = -1;
        public int previousY = -1;

        private bool actorInTheWay(pathing.TILE myMove, out string actorName)
        {
            bool actorInWay = false;
            actorName = "";
            foreach (ActorHandler.Actor myActor in TheActorHandler.ActorsHashTable.Values)
            {
                ActorHandler.position myPosition = TheActorHandler.GetUserPosition(myActor.id);
                if (myActor.pos.x == myMove.x && myActor.pos.y == myMove.y)
                {
                    actorName = TheActorHandler.GetUsernameFromID(myActor.id);
                    actorInWay = true;
                    break;
                }
            }
            return actorInWay;
        }
        private void makeMoves(int toX, int toY)
        {
            destX = toX;
            destY = toY;
            ActorHandler.position currentLocation = TheActorHandler.GetMyPosition();
            if (myThread.IsAlive)
            {
                //maybe I don't need to try to abort it, but just set keep moving to false, hrm...
                //keepMoving = false;
                myThread.Abort();
                myThread.Join();
                TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "Aborting movement and starting new path..."));
                Thread.Sleep(400);
            }
            myPathing.clearMap();
            myPathing.resetMap();
            currentLocation = TheActorHandler.GetMyPosition();
            myMoves = myPathing.getPath(currentLocation.x, currentLocation.y, toX, toY);
            if (myMoves.Count > 0)
            {
                //myPathing.resetMap();
                keepMoving = true;
                startX = currentLocation.x;
                startY = currentLocation.y;
                myThread = new Thread(new ThreadStart(makeMoves));
                myThread.Start();
                Thread.Sleep(0);
            }
            else
            {
                if (objectID > 0)
                {
                    gettingLocationInfo = true;
                    TheTCPWrapper.Send(CommandCreator.USE_MAP_OBJECT((uint)objectID, TheMySqlManager.getItemPos(useWithObject)));
                    useObjectPathing = true;
                    System.Threading.Thread.Sleep(3000);
                    TheTCPWrapper.Send(CommandCreator.LOCATE_ME());
                }
                else if (difference(currentLocation.x, toX) + difference(currentLocation.y, toY) < 12)
                {
                    useObjectPathing = false;
                    string actorName = "";
                    pathing.TILE targetLoc = new pathing.TILE();
                    targetLoc.x = toX;
                    targetLoc.y = toY;
                    if (actorInTheWay(targetLoc, out actorName))
                    {
                        moveLoc myLoc = new moveLoc();
                        myLoc.x = toX;
                        myLoc.y = toY;
                        myLoc = adjustMove(myLoc);
                        toX = myLoc.x;
                        toY = myLoc.y;
                    }
                    TheTCPWrapper.Send(CommandCreator.MOVE_TO(toX, toY));
                    string moveString = "Moving from " + currentLocation.x + "," + currentLocation.y;
                    moveString += " to end " + toX + "," + toY;
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(username, moveString));
                    keepGoing = false;
                    System.Threading.Thread.Sleep(4000);
                    TheTCPWrapper.Send(CommandCreator.LOCATE_ME());
                    //here here
                    if (commandGiven == "home")
                    {
                        Settings.IsTradeBot = true;
                        TradeHandler.storageOpen = false;
                        TradeHandler.openingStorage = false;
                    }
                    else
                    {
                        Settings.IsTradeBot = false;
                    }
                    myPathing.clearMap();
                }
                else
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "I cannot move to that location for some reason!"));
                }
            }
        }
        private struct moveLoc
        {
            public int x;
            public int y;
            public int distance;
        }
        private class mySorter : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                moveLoc myMoveLoc = (moveLoc)x;
                moveLoc myOtherMoveLoc = (moveLoc)y;
                return myOtherMoveLoc.distance.CompareTo(myMoveLoc.distance);
            }
        }
        private moveLoc adjustMove(moveLoc currentPos)
        {
            System.Collections.ArrayList myAdjustedMoves = new System.Collections.ArrayList();
            moveLoc adjustedMove = currentPos;
            int increment = 3; //number of squares around the spot to check
            for (int x = -increment; x <= increment; x++)
            {
                for (int y = -increment; y <= increment; y++)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }
                    else
                    {
                        //Console.WriteLine("{0},{1}", x, y);
                        moveLoc myMove = new moveLoc();
                        myMove.x = currentPos.x + x;
                        myMove.y = currentPos.y + y;
                        myMove.distance = Math.Max(Math.Abs(x), Math.Abs(y));
                        pathing.TILE myTile = myPathing.getTile(myMove.x, myMove.y);
                        if (myTile.z != 0)
                        {
                            myAdjustedMoves.Add(myMove);
                        }
                    }
                }
            }
            mySorter mySort = new mySorter();
            myAdjustedMoves.Sort(mySort);
            if (commandGiven != "storage")
            {
                myAdjustedMoves.Reverse();
            }
            foreach (moveLoc myMove in myAdjustedMoves)
            {
                //check to see if it's open, if so, return it and break
                pathing.TILE tempMove = new pathing.TILE();
                tempMove.x = myMove.x;
                tempMove.y = myMove.y;
                string actorName = "";
                if (!actorInTheWay(tempMove, out actorName))
                {
                    //need to validate the move on the map too
                    adjustedMove = (moveLoc)myMove;
                    break;
                }
                else
                {
                    Console.WriteLine("actor in the way: " + actorName + " " + tempMove.x + "," + tempMove.y);
                }
            }
            return adjustedMove;
        }
        private void makeMoves()
        {
            ActorHandler.position previousPosition = new ActorHandler.position();
            //pathing.TILE previousMove = (pathing.TILE)myMoves[0];
            string moveString = "";
            ActorHandler.position currentPosition = TheActorHandler.GetMyPosition();
            previousPosition = currentPosition;
            //start a loop through the moves array, it's our path
            foreach (pathing.TILE myMove in myMoves)
            {
                //we may want to save the "best" move along the way in case we don't find any moves that suit this condition...
                //if (difference(myMove.x, previousPosition.x) < 12 && difference(myMove.y, previousPosition.y) < 12)
                if (difference(myMove.x, previousPosition.x) + difference(myMove.y, previousPosition.y) < 12)
                {
                    continue;
                }
                else
                {
                    //this might be the place to check for actors in the way
                    //if one's in the way, just keep looking? hrm...
                    string actorName = "";
                    bool actorInWay = actorInTheWay(myMove, out actorName); ;
                    ActorHandler.position myPosition = TheActorHandler.GetMyPosition();
                    if (actorInWay)
                    {
                        Console.WriteLine("actor in the way: " + actorName + " " + myPosition.x + "," + myPosition.y);
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "trying to move around " + actorName));
                        continue;
                    }
                }
                currentPosition = TheActorHandler.GetMyPosition();
                moveString = "Moving from " + currentPosition.x + "," + currentPosition.y;
                moveString += " to " + myMove.x + "," + myMove.y;
                TheTCPWrapper.Send(CommandCreator.SEND_PM(username, moveString));
                TheTCPWrapper.Send(CommandCreator.MOVE_TO(myMove.x, myMove.y));
                //wait until the move is done or the time expires
                DateTime startTime = DateTime.Now;
                bool keepStepping = true;
                while (keepStepping && keepMoving)
                {
                    TimeSpan timeElapsed = DateTime.Now - startTime;
                    currentPosition = TheActorHandler.GetMyPosition();
                    if ((currentPosition.x == myMove.x && currentPosition.y == myMove.y) || timeElapsed.Seconds > 10)
                    {
                        //got to the spot, stop moving
                        keepStepping = false;
                    }
                    Thread.Sleep(10);
                }
                if (currentPosition.x == previousPosition.x && currentPosition.y == previousPosition.y && keepMoving)
                {
                    keepMoving = false;
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "I seem to be stuck :P"));
                }
                else
                {
                    previousPosition = currentPosition;
                }
                if (!keepMoving)
                {
                    break;
                }
            }
            //we'e either at the end by now, or we're stuck :P
            if (keepMoving)
            {
                pathing.TILE endMove = new pathing.TILE();
                string actorName = "";
                ActorHandler.position myPosition = TheActorHandler.GetMyPosition();
                endMove.x = destX;
                endMove.y = destY;
                bool actorInWay = actorInTheWay(endMove, out actorName);
                if (actorInWay && actorName.ToLower()!= TheActorHandler.GetUsernameFromID((short)TheActorHandler.MyActorID).ToLower())
                {
                    Console.WriteLine("actor in the way at final destination, move next to if possible: " + actorName + " " + destX + "," + destY);
                    moveLoc currentLoc = new moveLoc();
                    currentLoc.x = endMove.x;
                    currentLoc.y = endMove.y;
                    currentLoc.distance = 0;
                    moveLoc adjustedMove = adjustMove(currentLoc);
                    destX = adjustedMove.x;
                    destY = adjustedMove.y;
                }
                TheTCPWrapper.Send(CommandCreator.MOVE_TO(destX, destY));
                moveString = "Moving from " + currentPosition.x + "," + currentPosition.y;
                moveString += " to end " + destX + "," + destY;
                TheTCPWrapper.Send(CommandCreator.SEND_PM(username, moveString));
                DateTime startTime = DateTime.Now;
                bool keepStepping = true;
                while (keepStepping && keepMoving)
                {
                    TimeSpan timeElapsed = DateTime.Now - startTime;
                    currentPosition = TheActorHandler.GetMyPosition();
                    if ((currentPosition.x == destX && currentPosition.y == destY) || timeElapsed.Seconds > 10)
                    {
                        //got to the spot, stop moving
                        keepStepping = false;
                    }
                    Thread.Sleep(10);
                }
            }
            else
            {
                TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "Moving aborted."));
            }
            if (objectID > 0 && keepMoving)
            {
                gettingLocationInfo = true;
                TheTCPWrapper.Send(CommandCreator.USE_MAP_OBJECT((uint)objectID, TheMySqlManager.getItemPos(useWithObject)));
                useObjectPathing = true;
                System.Threading.Thread.Sleep(3000);
                TheTCPWrapper.Send(CommandCreator.LOCATE_ME());
                objectID = 0;
                //keepMoving = false;
            }
            else
            {
                useObjectPathing = false;
                keepGoing = false;
                if (keepMoving)
                {
                    currentPosition = TheActorHandler.GetMyPosition();
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "I should be at my final destination!"));
                    //experimental, not sure....
                    Thread.Sleep(1000);
                    gettingLocationInfo = true;
                    TheTCPWrapper.Send(CommandCreator.LOCATE_ME());
                    if (commandGiven == "home"  && MainClass.botType == 1)
                    {
                        MainClass.atHome = true;
                        Settings.IsTradeBot = true;
                    }
                }
                myPathing.clearMap();
            }
        }
        public bool useObjectPathing;
        public GotoCommandHandler(TCPWrapper MyTCPWrapper, BasicCommunication.MessageParser MyMessageParser, AdminHelpCommandHandler MyAdminHelpCommandHandler, MySqlManager MyMySqlManager, PMHandler MyPMHandler, ActorHandler MyActorHandler)
        {
            this.TheTCPWrapper = MyTCPWrapper;
            this.TheMessageParser = MyMessageParser;
            this.TheAdminHelpCommandHandler = MyAdminHelpCommandHandler;
            this.TheMySqlManager = MyMySqlManager;
            this.ThePMHandler = MyPMHandler;
            this.TheActorHandler = MyActorHandler;

            //if (CommandIsDisabled == false)
            {
                TheAdminHelpCommandHandler.AddCommand("#goto - Move the bot (long range)");

                TheMessageParser.Got_PM += new BasicCommunication.MessageParser.Got_PM_EventHandler(OnGotPM);
                TheMessageParser.Got_LocationInfo += new BasicCommunication.MessageParser.Got_LocationInfo_EventHandler(OnGotLocationInfo);
                TheTCPWrapper.GotCommand += new TCPWrapper.GotCommandEventHandler(OnGotCommand);
                myThread = new Thread(new ThreadStart(makeMoves));
            }

        }
        private void OnGotCommand(object sender, TCPWrapper.GotCommandEventArgs e)
        {
            //if (e.CommandBuffer[0] == 12 && keepGoing) //teleport in
            //{
            //    //System.Threading.Thread.Sleep(2000);
            //    if (!gettingLocationInfo)
            //    {
            //        lastMoveNumber = 0;
            //        gettingLocationInfo = true;
            //        TheTCPWrapper.Send(CommandCreator.LOCATE_ME());
            //    }
            //}
        }
        public static int lastMoveNumber = 0;
        public static bool gettingLocationInfo;
        private void OnGotLocationInfo(object sender, BasicCommunication.MessageParser.Got_LocationInfo_EventArgs e)
        {
            if (myThread.IsAlive)
            {
                myThread.Abort();
                myThread.Join();
            }
            objectID = 0;
            if (username != "")
            {
                TheTCPWrapper.Send(CommandCreator.SEND_PM(username, "I am in " + Settings.LocationInfo));
            }
            if (commandGiven != "" && keepGoing)
            {
                ActorHandler.position currentPosition = TheActorHandler.GetMyPosition();
                int myX = 0;
                int myY = 0;
                ActorHandler.position nextPosition = new ActorHandler.position();
                if (commandGiven == "home" && MainClass.mapName == MainClass.myHome.mapName)
                {
                    nextPosition = TheMySqlManager.getDestination(MainClass.mapName, commandGiven, out objectID, out useWithObject, ref lastMoveNumber);
                    if (nextPosition.x == -1)
                    {
                        myX = MainClass.myHome.x;
                        myY = MainClass.myHome.y;
                    }
                    else
                    {
                        myX = nextPosition.x;
                        myY = nextPosition.y;
                    }
                }
                else
                {
                    nextPosition = TheMySqlManager.getDestination(MainClass.mapName, commandGiven, out objectID, out useWithObject, ref lastMoveNumber);
                    myX = nextPosition.x;
                    myY = nextPosition.y;
                }
                makeMoves(myX, myY);
            }
            else if (commandGiven == "home")
            {
                ActorHandler.position currentPosition = TheActorHandler.GetMyPosition();
                orientSelf( currentPosition, TheTCPWrapper );
            }
            gettingLocationInfo = false;
        }
        public static void orientSelf(ActorHandler.position currentPosition, TCPWrapper TheTCPWrapper)
        {
            if (MainClass.myHome.heading != currentPosition.z_rot)
            {
                //MainClass.atHome = true;
                Console.WriteLine("orienting self");
                ActorHandler.orientingSelf = true;
                System.Threading.Thread.Sleep(1000);
                if (currentPosition.z_rot == 360)
                {
                    currentPosition.z_rot = 0;
                }
                int heading = MainClass.myHome.heading;
                if (MainClass.myHome.heading == 0 && currentPosition.z_rot > 180)
                {
                    heading = 360;
                }
                if ( heading < currentPosition.z_rot)
                {
                    //turn left
                    Console.WriteLine("turning left");
                    TheTCPWrapper.Send(CommandCreator.TURN_RIGHT());
                }
                else
                {
                    //turn right
                    Console.WriteLine("turning right");
                    TheTCPWrapper.Send(CommandCreator.TURN_LEFT());
                }
            }
        }
        //private Thread myThread = new Thread(new ThreadStart(makeMoves));
        private Thread myThread;
        public int objectID = 0;
        public int useWithObject;
        public bool keepGoing = true;
        public string commandGiven = "";
        private void OnGotPM(object sender, BasicCommunication.MessageParser.Got_PM_EventArgs e)
        {
            string Message = e.Message.ToLower();
            if (Message[0] != '#')
            {
                Message = "#" + Message;
            }

            Message = Message.Replace(',', ' ');
            string[] CommandArray = Message.Split(' ');

            if (CommandArray[0] == "#goto")
            {
                if (CommandArray.Length < 2)
                //if (CommandArray.Length < 2 || CommandArray[1].Contains(","))
                {
                    goto WrongArguments;
                }

                bool disabled = TheMySqlManager.CheckIfCommandIsDisabled("#goto", Settings.botid);

                if (disabled == true)
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "This command is disabled"));
                    return;
                }

                if (TheMySqlManager.GetUserRank(e.username, Settings.botid) < TheMySqlManager.GetCommandRank("#goto", Settings.botid))
                {
                    TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "You are not authorized to use this command!"));
                    return;
                }
                TradeHandler.storageOpen = false;
                username = e.username;
                keepGoing = false;
                objectID = 0;
                commandGiven = "";
                TheMySqlManager.getHomeInfo();
                ActorHandler.position currentLocation = TheActorHandler.GetMyPosition();
                int myX = -1;
                int myY = -1;
                string searchWord = "";
                //Console.WriteLine(CommandArray.Length);
                if (CommandArray.Length == 3)
                {
                    try
                    {
                        myX = int.Parse(CommandArray[1]);
                        myY = int.Parse(CommandArray[2]);
                    }
                    catch
                    {
                    }
                    finally
                    {
                    }
                }
                //loading the map, hold on a sec...

                //write out the values here, something's wrong...
                //Console.WriteLine((searchWord == "" && myX >= 0 && myY >= 0));

                if (searchWord == "" && myX >= 0 && myY >= 0)
                {
                    makeMoves(myX, myY);
                }
                else if ((string)CommandArray.GetValue(1) == "abort")
                {
                    keepMoving = false;
                }
                else if ((string)CommandArray.GetValue(1) == "storage" || (string)CommandArray.GetValue(1) == "home")
                {
                    ActorHandler.position currentPosition = TheActorHandler.GetMyPosition();
                    commandGiven = (string)CommandArray.GetValue(1);
                    lastMoveNumber = 0;
                    if (commandGiven == "storage")
                    {
                        Settings.IsTradeBot = false;
                        MainClass.atHome = false;
                    }
                    keepGoing = true;
                    //need to find our path to storage... this path will be a series of maps and exits to take to get to the next map
                    //may be able to generalize this to any destination (storage, home, whatever...)
                    //probably wanna do a loop here, looking for the destinations until we find our final map
                    //the map change will use the fact that we're moving to storage/home to set our destination
                    ActorHandler.position nextPosition = new ActorHandler.position();
                    if (commandGiven == "home" && MainClass.mapName == MainClass.myHome.mapName)
                    {
                        nextPosition = TheMySqlManager.getDestination(MainClass.mapName, commandGiven, out objectID, out useWithObject, ref lastMoveNumber);
                        if (nextPosition.x == -1)
                        {
                            myX = MainClass.myHome.x;
                            myY = MainClass.myHome.y;
                        }
                        else
                        {
                            myX = nextPosition.x;
                            myY = nextPosition.y;
                        }
                    }
                    else
                    {
                        nextPosition = TheMySqlManager.getDestination(MainClass.mapName, commandGiven, out objectID, out useWithObject, ref lastMoveNumber);
                        myX = nextPosition.x;
                        myY = nextPosition.y;
                    }
                    makeMoves(myX, myY);
                }
                else
                {
                    myPathing.resetMap();
                    ArrayList myMatches = new ArrayList();
                    if (CommandArray.Length == 3)
                    {
                        searchWord = (string)CommandArray.GetValue(2);
                        myMatches = myPathing.listObjects(searchWord);
                    }
                    else
                    {
                        myMatches = myPathing.listObjects(currentLocation);
                    }
                    //list objects with the serach word in them
                    foreach (pathing.object3D myMatch in myMatches)
                    {
                        string filename = new string(myMatch.objectFileName).Replace('\0', ' ');
                        filename = filename.Substring(filename.IndexOf(searchWord));
                        int thisX = (int)Math.Round(myMatch.x_pos, 0);
                        int thisY = (int)Math.Round(myMatch.y_pos, 0);
                        string outputLine = myMatch.object_id + "|" + filename.Trim() + "|" + thisX + "," + thisY;
                        TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, outputLine));
                    }
                }
            }
            return;
        WrongArguments:
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Here is the usage of the #goto command:   "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[#goto x y (or goto x,y)                   "));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[------------------------------------------"));
            TheTCPWrapper.Send(CommandCreator.SEND_PM(e.username, "[Example: #goto 192,168                    "));
            return;
        }

    }
}