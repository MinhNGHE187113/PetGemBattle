using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PetGemBattle
{
    [Serializable]
    public sealed class BattleJournal
    {
        public int version=1,seed;
        public string balance;
        public List<Entry> entries=new List<Entry>();
        [Serializable] public sealed class Entry
        {
            public string command;
            public int turn;
            public int[] board,stats;
            public List<BattleEvent> events;
            public List<double> rolls;
        }
        public void Record(string command,int turn,MatchBoard board,PetState player,PetState enemy,BattleReport report=null,List<double> rolls=null)
        {
            entries.Add(new Entry {command=command,turn=turn,board=board.Snapshot(),stats=BattleEvent.Capture(player,enemy),
                events=report==null ? new List<BattleEvent>() : new List<BattleEvent>(report.Events),rolls=rolls==null ? new List<double>() : new List<double>(rolls)});
        }
        public string Save()
        {
            string directory=Path.Combine(Application.persistentDataPath,"BattleLogs");Directory.CreateDirectory(directory);
            string path=Path.Combine(directory,"latest-battle.json"),temp=path+".tmp";
            File.WriteAllText(temp,JsonUtility.ToJson(this,true));
            if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);
            return path;
        }
    }
    public sealed class RecordingBattleRandom : IBattleRandom
    {
        private readonly IBattleRandom source;
        public readonly List<double> Rolls=new List<double>();
        public RecordingBattleRandom(int seed){source=new SeededBattleRandom(seed);}
        public double Next(){double value=source.Next();Rolls.Add(value);return value;}
    }
}
