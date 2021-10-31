﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace P03_FootballBetting.Data.Models
{
    public class PlayerStatistic
    {
        [ForeignKey(nameof(Game))]

        public int GameId { get; set; }
        public Game Game { get; set; }

        [ForeignKey(nameof(Player))]

        public int PlayerId { get; set; }
        public Player Player { get; set; }

        public byte ScoredGoals { get; set; }
        public byte Assists { get; set; }
        public byte MinutesPlayed { get; set; }
    }
}
