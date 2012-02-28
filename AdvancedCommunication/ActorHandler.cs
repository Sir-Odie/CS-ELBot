// Eternal Lands Bot
// Copyright (C) 2006  Artem Makhutov
// artem@makhutov.org
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

namespace cs_elbot.AdvancedCommunication
{
	/// <summary>
	/// description of ActorHandler.
	/// </summary>
	public class ActorHandler
	{
		// OnAddNewActor
		public delegate void AddNewActorEventHandler(object sender, AddNewActorEventArgs e);
		
		public class AddNewActorEventArgs : EventArgs
		{
			public readonly Actor NewActor;
			public AddNewActorEventArgs(Actor NewActor)
		    {
		        this.NewActor = NewActor;
		    }
		}
			
		public event AddNewActorEventHandler AddNewActor;
		
		private void OnAddNewActor(AddNewActorEventArgs e)
		{
			if(AddNewActor!=null)
            	AddNewActor(this,e);
		}
		
		public class Actor
		{
			public Int16 id;
			public string name;
			public string guild;
			public position pos;
            public byte buffs;
			public int max_health;
			public int cur_health;
			public int actor_type;
			public int skin;
			public int hair;
			public int shirt;
			public int pants;
			public int boots;
			public int frame;
			public int cape;
			public int head;
			public int shield;
			public int weapon;
			public int helmet;
			public bool dead;
			public int kind_of_actor;
            public int threatLevel;
		}
		
		public struct position
		{
			public int x;
			public int y;
			public int z;
			public int z_rot;
		}

        public int MyActorID;
        public static bool orientingSelf = false;
//		int MyActorID;
		Actor MyActor;
		
		public System.Collections.Hashtable ActorsHashTable = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());

		private TCPWrapper TheTCPWrapper;
		private Logger TheLogger;
        private MySqlManager TheMySQLManager;

        public void setActorThreatLevel(Actor myActor, int threatLevel)
        {
            if (ActorsHashTable.Contains(myActor.id))
            {
                Actor tempActor = (Actor)ActorsHashTable[myActor.id];
                tempActor.threatLevel = threatLevel;
                ActorsHashTable[myActor.id] = tempActor;
            }
        }
		private void OnGotCommand(object sender, TCPWrapper.GotCommandEventArgs e)
		{
			byte[] buffer = e.CommandBuffer;
			byte cmd = e.CommandBuffer[0];
			switch (cmd)
			{
				//ADD_NEW_ACTOR (NPC)
				case 0x01:
					ADD_NEW_ACTOR(buffer);	
					break;
				
				//ADD_ACTOR_COMMAND
				case 0x02:
					ADD_ACTOR_COMMAND(buffer);	
					break;
						
				//ADD_NEW_ENHANCED_ACTOR
				case 0x33:
					ADD_NEW_ENHANCED_ACTOR(buffer);
					break;
					
				//REMOVE_ACTOR
				case 0x06:
					REMOVE_ACTOR(buffer);
					break;
					
				//YOU_ARE
				case 0x03:
					//user_id = System.BitConverter.ToInt16(buffer,3);
					YOU_ARE(buffer);
					break;
				
				//KILL_ALL_ACTORS
				case 0x09:
					KILL_ALL_ACTORS(buffer);
					break;

                //SEND_BUFFS
                case 78:
                    UPDATE_BUFFS(buffer);
                    break;
			}
		}
		
		public ActorHandler(TCPWrapper MyTCPWrapper, Logger MyLogger, MySqlManager MyMySqlManager)
		{
			this.TheTCPWrapper = MyTCPWrapper;
			this.TheLogger = MyLogger;
            this.TheMySQLManager = MyMySqlManager;

			TheTCPWrapper.GotCommand += new TCPWrapper.GotCommandEventHandler(OnGotCommand);
		}
		
		private void Add_Actor(Actor new_actor)
		{
            //if (ActorsHashTable.Contains(new_actor.id) == false)
            if (ActorsHashTable.Contains(new_actor.id) == false)
			{
			    ActorsHashTable.Add(new_actor.id,new_actor);
			}
			else
			{
				ActorsHashTable[new_actor.id] = new_actor;
			}
            if (new_actor.id == MyActorID)
            {
                MyActor = new_actor;
            }
        }
		
        private void UPDATE_BUFFS(byte[] buffer)
        {
            UInt16 id = System.BitConverter.ToUInt16(buffer, 3);
            if (id > 1000)
                return;
            int buffs = (int)buffer[4];
            //Actor tempactor;
            foreach (Actor tempactor in ActorsHashTable.Values)
            {
                if (tempactor.id == id)
                {
                    tempactor.buffs = buffer[4];
                    if (tempactor.buffs != 0)
                    {
                        //TheTCPWrapper.Send(CommandCreator.SEND_PM("dogbreath", "Invisibility Alert: " + tempactor.name.Trim() + " is invisible and sneaking around at " + tempactor.pos.x.ToString() + "," + tempactor.pos.y.ToString()));
                    }
                    break;
                }
            }
        }

		private void Del_Actor(Int16 id)
		{
			if (ActorsHashTable.Contains(id)==true)
			{
				Actor MyActor = (Actor)ActorsHashTable[id];
				ActorsHashTable.Remove(id);

			}
		}
		
		public position GetUserPosition(Int16 id)
		{
			position pos;
			
			pos.x=0;
			pos.y=0;
			pos.z=0;
			pos.z_rot=0;
			
			Actor TempActor;
			
			if (ActorsHashTable.Contains(id)==true)
			{
				TempActor = (Actor)ActorsHashTable[id];
				return TempActor.pos;
			}
			
			return pos;
		}
		
		public string GetUsernameFromID(Int16 id)
		{
			Actor TempActor;
			
			if (ActorsHashTable.Contains(id)==true)
			{
				TempActor = (Actor)ActorsHashTable[id];
				return TempActor.name;
			}
			
			return "";
		}
		
		public Int16 GetUserIDFromname(string name)
		{
			lock (ActorsHashTable.SyncRoot)
			{
				foreach (Actor TempActor in ActorsHashTable.Values)
				{
					if (TempActor.name.ToLower() == name.ToLower())
					{
						return TempActor.id;
					}
				}
			}
			return -1;
		}
		
		public position GetMyPosition()
		{
			return MyActor.pos;
		}
		
		public Actor GetMyData()
		{
			return MyActor;
		}
		
		private void KILL_ALL_ACTORS(byte[] Player_Buffer)
		{
			ActorsHashTable.Clear();
		}
		
		private void REMOVE_ACTOR(byte[] Player_Buffer)
		{
			Int16 id = 0;
			int len = System.BitConverter.ToInt16(Player_Buffer,1);

			// REMOVE_ACTOR can remove multiple actors
			for (int i=3;i<=len;i=i+2)
			{
				id = System.BitConverter.ToInt16(Player_Buffer,i);
				Del_Actor(id);
			}
		}
		
		private void ADD_NEW_ACTOR (byte[] Player_Buffer)
		{
			Actor new_actor = new Actor();
            new_actor.threatLevel = 0;
			
			new_actor.id=System.BitConverter.ToInt16(Player_Buffer,3);
			new_actor.pos.x = System.BitConverter.ToInt16(Player_Buffer,5);
			new_actor.pos.y = System.BitConverter.ToInt16(Player_Buffer,7);
			new_actor.pos.z = System.BitConverter.ToInt16(Player_Buffer,9);
			new_actor.pos.z_rot = System.BitConverter.ToInt16(Player_Buffer,11);

			new_actor.actor_type=(int)Player_Buffer[13];
			new_actor.frame=(int)Player_Buffer[14];
			new_actor.max_health=System.BitConverter.ToInt16(Player_Buffer,15);
			new_actor.cur_health=System.BitConverter.ToInt16(Player_Buffer,17);
			
			new_actor.kind_of_actor=(int)Player_Buffer[19];
			
			switch (new_actor.frame)
			{
				//frame_die1
				case 2:
					new_actor.dead=true;
					break;
				//frame_die2
				case 3:
					new_actor.dead=true;
					break;
			}
			
			string name="";
			string name_buffer = System.Text.Encoding.ASCII.GetString(Player_Buffer,20,Player_Buffer.Length-21);
			
			// remove bad some chars (eg color tags)
			for (int i=0;i<name_buffer.Length;i++)
			{
				if (!(name_buffer[i]<32 || name_buffer[i]==63 || name_buffer[i]>126))
				{
					name = name+name_buffer[i];
				}
			}
			
			new_actor.name = name;
			
			Add_Actor(new_actor);
        }
	
		private void ADD_NEW_ENHANCED_ACTOR (byte[] Player_Buffer)
		{
			int i = 0;
			string name = "";
            int buffs = 0;
			string[] nameguild;
			
			Actor new_actor = new Actor();
			
			new_actor.id=System.BitConverter.ToInt16(Player_Buffer,3);
			new_actor.pos.x = System.BitConverter.ToInt16(Player_Buffer,5) & 0x7ff;
            new_actor.pos.y = System.BitConverter.ToInt16(Player_Buffer, 7) & 0x7ff;
            i = System.BitConverter.ToUInt16(Player_Buffer, 3) & 0xF800;
            i = (i>>11) & 0x01f;
            buffs = i;
            i = System.BitConverter.ToUInt16(Player_Buffer, 5) & 0xF800;
            i = ((i >> 11) & 0x01f) << 5;
            buffs += i;
            new_actor.buffs = (byte)buffs;
			new_actor.pos.z = System.BitConverter.ToInt16(Player_Buffer,9);
			new_actor.pos.z_rot = System.BitConverter.ToInt16(Player_Buffer,11);

			new_actor.actor_type=(int)Player_Buffer[13];
            new_actor.skin = (int)Player_Buffer[15];
			new_actor.hair=(int)Player_Buffer[16];
			new_actor.shirt=(int)Player_Buffer[17];
			new_actor.pants=(int)Player_Buffer[18];
			new_actor.boots=(int)Player_Buffer[19];
			new_actor.head=(int)Player_Buffer[20];
			new_actor.shield=(int)Player_Buffer[21];
			new_actor.weapon=(int)Player_Buffer[22];
			new_actor.cape=(int)Player_Buffer[23];
			new_actor.helmet=(int)Player_Buffer[24];
			new_actor.frame=(int)Player_Buffer[25];
			
			new_actor.max_health = System.BitConverter.ToInt16(Player_Buffer,26);
			new_actor.cur_health = System.BitConverter.ToInt16(Player_Buffer,28);
			new_actor.kind_of_actor = (int)Player_Buffer[30];

            //string name_buffer = System.Text.Encoding.ASCII.GetString(Player_Buffer,31,Player_Buffer.Length-32);
            string name_buffer = "";
            for (int count = 0; count < Player_Buffer.Length - 32; count++)
            {
                name_buffer+=Convert.ToChar(Player_Buffer[count+31]);
            }
            //string name_buffer = System.Text.Encoding.BigEndianUnicode.GetString(Player_Buffer, 31, Player_Buffer.Length - 32);
            //string name_buffer = System.Text.Encoding.UTF32.GetString(Player_Buffer, 31, Player_Buffer.Length - 32);
			// remove the zoom factor from name
            if (name_buffer.LastIndexOf((char)0) != -1)
			{
				name_buffer = name_buffer.Remove(name_buffer.LastIndexOf((char)0));
			}
			TheLogger.Debug("ADD_NEW_ENHANCED_ACTOR: " + name_buffer + "\n");
            //if ((buffs)!=0 && new_actor.id<1001)// == 1 && Settings.Loginname.ToLower()=="agneum")
            //{
            // invis check, maybe needed for guard bots
            //}			
			
			// remove bad some chars (eg color tags)
			for (i=0;i<name_buffer.Length;i++)
			{
                if (!(name_buffer[i] < 32 || name_buffer[i] == 63 || (name_buffer[i]> 126 && name_buffer[i] < 191)))
                {
                    name = name + name_buffer[i];
                }
			}

			nameguild = name.Split(' ');
			
			if (nameguild.Length==1)
			{
				new_actor.name = nameguild[0];
				new_actor.guild = "";
            }
			else
			{
				new_actor.name = nameguild[0];
				new_actor.guild = nameguild[1];
            }

			switch (new_actor.frame)
			{
				//frame_die1
				case 2:
					new_actor.dead=true;
					break;
				//frame_die2
				case 3:
					new_actor.dead=true;
					break;
			}
			
			Add_Actor(new_actor);
			OnAddNewActor(new AddNewActorEventArgs(new_actor));
            TheMySQLManager.UpdatePlayer(new_actor.name, new_actor.guild);
			//NotifyGossip(new_actor);
            if (new_actor.id == MyActorID)
            {
                TheMySQLManager.getHomeInfo();
                MyActor = new_actor;
                if (MyActor.pos.z_rot == 360)
                {
                    MyActor.pos.z_rot = 0;
                }
                if (AdvancedCommunication.GotoCommandHandler.pathing.difference(MainClass.myHome.x, MyActor.pos.x) < 5 && AdvancedCommunication.GotoCommandHandler.pathing.difference(MainClass.myHome.y, MyActor.pos.y) < 5)
                {
                    MainClass.atHome = true;
                    if (MainClass.myHome.heading != MyActor.pos.z_rot && MainClass.mapName == MainClass.myHome.mapName)
                    {
                        //orient self
                        if (MyActor.pos.z_rot == MainClass.myHome.heading)
                        {
                            orientingSelf = false;
                        }
                        else
                        {
                            orientingSelf = true;
                            System.Threading.Thread.Sleep(1000);
                            if (MainClass.myHome.heading < MyActor.pos.z_rot)
                            {
                                //turn left
                                TheTCPWrapper.Send(CommandCreator.TURN_RIGHT());
                            }
                            else
                            {
                                //turn right
                                TheTCPWrapper.Send(CommandCreator.TURN_LEFT());
                            }
                            //System.Threading.Thread.Sleep(1000);
                        }
                    }
                }
                else
                {
                    Settings.IsTradeBot = false;
                }
            }

		}
		
        //private void NotifyGossip (Actor NewActor)
        //{
        //    //Send a feed of guildtags / usernames to gossip
        //    if (TheMySQLManager.guildfeedgossip(Settings.botid)==true)
        //    {
        //        if (NewActor.guild!="")
        //        {
        //            // Ignore Ants
        //            if (NewActor.guild.ToLower()!="ant" && NewActor.guild.ToLower()!="red")
        //            {
        //                TheTCPWrapper.Send(CommandCreator.SEND_PM("gossip","gf " + NewActor.name + " " + NewActor.guild));
        //            }
        //        }
        //        else
        //        {
        //            TheTCPWrapper.Send(CommandCreator.SEND_PM("gossip","gf " + NewActor.name));
        //        }
        //    }
        //}
		private void GET_ACTOR_DAMAGE (byte[] Player_Buffer)
		{
			
			Int16 id = System.BitConverter.ToInt16(Player_Buffer,3);
			Int16 amount = System.BitConverter.ToInt16(Player_Buffer,5);
			
			if (ActorsHashTable.Contains(id))
		    {
				Actor TempActor = (Actor)ActorsHashTable[id];
				TempActor.cur_health-=amount;
				ActorsHashTable[id] = TempActor;
				
				if (this.MyActorID==id)
				{
					MyActor.cur_health-=amount;
				}
		    }
		}
		
		private void GET_ACTOR_HEAL (byte[] Player_Buffer)
		{
			
			Int16 id = System.BitConverter.ToInt16(Player_Buffer,3);
			Int16 amount = System.BitConverter.ToInt16(Player_Buffer,5);
			
			if (ActorsHashTable.Contains(id))
		    {
				Actor TempActor = (Actor)ActorsHashTable[id];
				TempActor.cur_health+=amount;
				ActorsHashTable[id] = TempActor;
				
				if (this.MyActorID==id)
				{
					MyActor.cur_health+=amount;
				}
		    }
		}
		
		private void ADD_ACTOR_COMMAND (byte[] Player_Buffer)
		{
            int len = System.BitConverter.ToInt16(Player_Buffer, 1);
			Int16 actor_id = 0;
			byte cmd = 0;

			for (int i=3;i<=len;i=i+3)
			{
				actor_id = System.BitConverter.ToInt16(Player_Buffer,i);
				cmd = Player_Buffer[i+2];
				execute_actor_command(cmd,actor_id);
            }
		}
		
		private void execute_actor_command (byte cmd, Int16 id)
		{
			if (ActorsHashTable.ContainsKey(id))
			{
                Actor My_Actor = (Actor)ActorsHashTable[id];
				Actor MyNewActor = switch_enhanced_actor_command(My_Actor,cmd);
                ActorsHashTable[id] = MyNewActor;
            }	
		}
		
		private Actor switch_enhanced_actor_command (Actor My_Actor, int cmd)
		{
            switch (cmd)
			{
				//nothing
				case 0:
					break;
				//kill_me
				case 1:
					break;
				//die1
				case 3:
					My_Actor.dead=true;
					My_Actor.frame=2;

					break;
				//die2
				case 4:
					My_Actor.dead=true;
					My_Actor.frame=3;

					break;
				//pain1
				case 5:
					My_Actor.frame=4;

					break;
				//pain2
				case 17:
					My_Actor.frame=11;

					break;
				//pick
				case 6:
					My_Actor.frame=5;

					break;
				//drop
				case 7:
					My_Actor.frame=6;
					
					break;
				//idle
				case 8:
					My_Actor.frame=7;
					
					break;
				//harvest
				case 9:
					My_Actor.frame=8;

					break;
				//cast
				case 10:
					My_Actor.frame=9;

					break;
				//ranged
				case 11:
					My_Actor.frame=10;

					break;
				//meele
				case 12:
					break;
				//sit_down
				case 13:
					My_Actor.frame=12;

					break;
				//stand_up
				case 14:
					My_Actor.frame=13;

					break;
				//turn_right
				case 15:
                    if (MyActor.pos.z_rot == 315)
                    {
                        My_Actor.pos.z_rot = 0;
                    }
                    else
                    {
                        My_Actor.pos.z_rot += 45;
                    }
                    if (MyActor.id == My_Actor.id && orientingSelf)
                    {
                        if (My_Actor.pos.z_rot == MainClass.myHome.heading)
                        {
                            orientingSelf = false;
                        }
                        else
                        {
                            orientingSelf = true;
                            System.Threading.Thread.Sleep(1000);
                            //turn right
                            TheTCPWrapper.Send(CommandCreator.TURN_LEFT());
                        }
                    }
                    break;
				//turn_left
				case 16:
                    if (MyActor.pos.z_rot == 0)
                    {
                        MyActor.pos.z_rot = 315;
                    }
                    else
                    {
                        My_Actor.pos.z_rot -= 45;
                    }
                    if (MyActor.id == My_Actor.id && orientingSelf)
                    {
                        if (My_Actor.pos.z_rot == MainClass.myHome.heading)
                        {
                            orientingSelf = false;
                        }
                        else
                        {
                            orientingSelf = true;
                            System.Threading.Thread.Sleep(1000);
                            //turn left
                            TheTCPWrapper.Send(CommandCreator.TURN_RIGHT());
                        }
                    }
					break;
					
					
				//enter_combat
				case 18:
					My_Actor.frame=16;

					break;
				//leave_combat
				case 19:
					My_Actor.frame=17;

					break;
				
				
				//move_n
				case 20:
					My_Actor.frame=0;
					My_Actor.pos.y++;
                    My_Actor.pos.z_rot = 0;

					break;
				//move_ne
				case 21:
					My_Actor.frame=0;
					My_Actor.pos.y++;
					My_Actor.pos.x++;
                    My_Actor.pos.z_rot = 45;

					break;
				//move_e
				case 22:
					My_Actor.frame=0;
					My_Actor.pos.x++;
                    My_Actor.pos.z_rot = 90;

					break;
				//move_se
				case 23:
					My_Actor.frame=0;
					My_Actor.pos.y--;
					My_Actor.pos.x++;
                    My_Actor.pos.z_rot = 135;

					break;
				//move_s
				case 24:
					My_Actor.frame=0;
					My_Actor.pos.y--;
                    My_Actor.pos.z_rot = 180;

					break;
				//move_sw
				case 25:
					My_Actor.frame=0;
					My_Actor.pos.y--;
					My_Actor.pos.x--;
                    My_Actor.pos.z_rot = 225;

					break;
				//move_w
				case 26:
					My_Actor.frame=0;
					My_Actor.pos.x--;
                    My_Actor.pos.z_rot = 270;

					break;
				//move_nw
				case 27:
					My_Actor.frame=0;
					My_Actor.pos.y++;
					My_Actor.pos.x--;
                    My_Actor.pos.z_rot = 315;

					break;
				
				
				//run_n
				case 30:
					My_Actor.frame=1;
					My_Actor.pos.y++;

					break;
				//run_ne
				case 31:
					My_Actor.frame=1;
					My_Actor.pos.y++;
					My_Actor.pos.x++;

					break;
				//run_e
				case 32:
					My_Actor.frame=1;
					My_Actor.pos.x++;

					break;
				//run_se
				case 33:
					My_Actor.frame=1;
					My_Actor.pos.y--;
					My_Actor.pos.x++;

					break;
				//run_s
				case 34:
					My_Actor.frame=1;
					My_Actor.pos.y--;

					break;
				//run_sw
				case 35:
					My_Actor.frame=1;
					My_Actor.pos.y--;
					My_Actor.pos.x--;

					break;
				//run_w
				case 36:
					My_Actor.frame=1;
					My_Actor.pos.x--;

					break;
				//run_nw
				case 37:
					My_Actor.frame=1;
					My_Actor.pos.y++;
					My_Actor.pos.x--;

					break;
				
				//turn_n
				case 38:
					break;
				//turn_ne
				case 39:
					break;
				//turn_e
				case 40:
					break;
				//turn_se
				case 41:
					break;
				//turn_s
				case 42:
					break;
				//turn_sw
				case 43:
					break;
				//turn_w
				case 44:
					break;
				//turn_nw
				case 45:
					break;
				
				//attack_up_1
				case 46:
					My_Actor.frame=18;

					break;
				//attack_up_2
				case 47:
					My_Actor.frame=19;

					break;
				//attack_up_3
				case 48:
					My_Actor.frame=20;

					break;
				//attack_up_4
				case 49:
					My_Actor.frame=21;
					
					break;
				//attack_down_1
				case 50:
					My_Actor.frame=22;

					break;
				//attack_down_2
				case 51:
					My_Actor.frame=23;

					break;
                default:
                    break;
			}
			
			return My_Actor;
		}
		
		private void YOU_ARE (byte[] Player_Buffer)
		{
			MyActorID=System.BitConverter.ToInt16(Player_Buffer,3);
		}
		
		public string GetFrameString (int frame_id)
		{
			int i;
			string[] frame = new string[256];
			
			for (i=0;i<256;i++)
			{
				frame[i]="Unknown "+i.ToString();
			}
			
			frame[0]="walk";
			frame[1]="run";
			frame[2]="die1";
			frame[3]="die2";
			frame[4]="pain1";
			frame[11]="pain2";
			frame[5]="pick";
			frame[6]="drop";
			frame[7]="idle";
			frame[8]="harvest";
			frame[9]="cast";
			frame[10]="ranged";
			frame[12]="sit";
			frame[13]="stand";
			frame[14]="sit_idle";
			frame[15]="combat_idle";
			frame[16]="in_combat";
			frame[17]="out_combat";
			frame[18]="attack_up_1";
			frame[19]="attack_up_2";
			frame[20]="attack_up_3";
			frame[21]="attack_up_4";
			frame[22]="attack_down_1";
			frame[23]="attack_down_2";
			
			return frame[frame_id];
		}
		
		public string GetActorTypeString (int Actor_Type_ID)
		{
			int i;
			string[] Actor_Type = new string[256];
			
			for (i=0;i<256;i++)
			{
				Actor_Type[i]="Unknown "+i.ToString();
			}
			Actor_Type[0]="human_female";
			Actor_Type[1]="human_male";
			Actor_Type[2]="elf_female";
			Actor_Type[3]="elf_male";
			Actor_Type[4]="dwarf_female";
			Actor_Type[5]="dwarf_male";
			Actor_Type[6]="wraith";
			Actor_Type[7]="cyclops";
			Actor_Type[8]="beaver";
			Actor_Type[9]="rat";
			Actor_Type[10]="goblin_male_2";
			Actor_Type[11]="goblin_female_1";
			Actor_Type[12]="town_folk4";
			Actor_Type[13]="town_folk5";
			Actor_Type[14]="shop_girl3";
			Actor_Type[15]="deer";
			Actor_Type[16]="bear";
			Actor_Type[17]="wolf";
			Actor_Type[18]="white_rabbit";
			Actor_Type[19]="brown_rabbit";
			Actor_Type[20]="boar";
			Actor_Type[21]="bear2";
			Actor_Type[22]="snake1";
			Actor_Type[23]="snake2";
			Actor_Type[24]="snake3";
			Actor_Type[25]="fox";
			Actor_Type[26]="puma";
			Actor_Type[27]="ogre_male_1";
			Actor_Type[28]="goblin_male_1";
			Actor_Type[29]="orc_male_1";
			Actor_Type[30]="orc_female_1";
			Actor_Type[31]="skeleton";
			Actor_Type[32]="gargoyle1";
			Actor_Type[33]="gargoyle2";
			Actor_Type[34]="gargoyle3";
			Actor_Type[35]="troll";
			Actor_Type[36]="chimeran_wolf_mountain";
			Actor_Type[37]="gnome_female";
			Actor_Type[38]="gnome_male";
			Actor_Type[39]="orchan_female";
			Actor_Type[40]="orchan_male";
			Actor_Type[41]="draegoni_female";
			Actor_Type[42]="draegoni_male";
			Actor_Type[43]="skunk_1";
			Actor_Type[44]="racoon_1";
			Actor_Type[45]="unicorn_1";
			Actor_Type[46]="chimeran_wolf_desert";
			Actor_Type[47]="chimeran_wolf_forest";
			Actor_Type[48]="bear_3";
			
			return Actor_Type[Actor_Type_ID];
		}
	}
}
