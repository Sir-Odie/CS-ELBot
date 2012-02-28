-- MySQL dump 10.11
--
-- Host: localhost    Database: testbot
-- ------------------------------------------------------
-- Server version	5.0.37-community-nt

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `advertlog`
--

DROP TABLE IF EXISTS `advertlog`;
CREATE TABLE `advertlog` (
  `id` bigint(20) NOT NULL auto_increment,
  `adverttext` text NOT NULL,
  `adverttime` timestamp NOT NULL default CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP,
  `botid` bigint(20) NOT NULL,
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=41303 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;

--
-- Table structure for table `adverts`
--

DROP TABLE IF EXISTS `adverts`;
CREATE TABLE `adverts` (
  `id` bigint(20) NOT NULL auto_increment,
  `botid` bigint(20) NOT NULL,
  `adverttext` text NOT NULL,
  `disabled` int(11) NOT NULL default '0',
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=21 DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

--
-- Table structure for table `botconfig`
--

DROP TABLE IF EXISTS `botconfig`;
CREATE TABLE `botconfig` (
  `botid` int(10) unsigned NOT NULL,
  `invfiller` char(1) NOT NULL default '~',
  `wantedfiller` char(1) NOT NULL default '~',
  `showzeroprice` tinyint(1) unsigned NOT NULL default '0',
  PRIMARY KEY  (`botid`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Table structure for table `botfile`
--

DROP TABLE IF EXISTS `botfile`;
CREATE TABLE `botfile` (
  `id` int(10) unsigned NOT NULL auto_increment,
  `botid` tinyint(3) unsigned NOT NULL,
  `chatlog` blob,
  `chatlogname` varchar(16) default NULL,
  `chatlogtype` varchar(16) default NULL,
  `chatlogsize` int(10) unsigned default NULL,
  PRIMARY KEY  (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Table structure for table `botgroups`
--

DROP TABLE IF EXISTS `botgroups`;
CREATE TABLE `botgroups` (
  `id` bigint(20) NOT NULL auto_increment,
  `name` text NOT NULL,
  `username` text NOT NULL,
  `password` text NOT NULL,
  `comments` text NOT NULL,
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

--
-- Table structure for table `bots`
--

DROP TABLE IF EXISTS `bots`;
CREATE TABLE `bots` (
  `id` bigint(20) NOT NULL auto_increment,
  `name` varchar(45) NOT NULL,
  `botid` bigint(20) NOT NULL,
  `bot_x` int(11) NOT NULL default '-1',
  `bot_y` int(11) NOT NULL default '-1',
  `comments` text NOT NULL,
  `enabledebuging` tinyint(4) NOT NULL default '0',
  `loggotpms` tinyint(4) NOT NULL default '1',
  `logsendpms` tinyint(4) NOT NULL default '1',
  `sendreceivedpms` tinyint(4) NOT NULL default '1',
  `pmmonitorlist` text NOT NULL,
  `senderrorpms` tinyint(4) NOT NULL default '1',
  `locationdescription` text NOT NULL,
  `welcomedescription` text NOT NULL,
  `infodescription` text NOT NULL,
  `botowner` text NOT NULL,
  `guildlongname` text NOT NULL,
  `guildfeedgossip` tinyint(4) NOT NULL default '0',
  `advertchannel` bigint(20) NOT NULL default '3',
  `lastadverttime` datetime NOT NULL,
  `advertise` tinyint(4) NOT NULL default '1' COMMENT 'Should we allow this bot to advertise on the market channel?',
  `advertendmessage` text NOT NULL COMMENT '[inv/loc/wanted]',
  `minadverttime` bigint(20) NOT NULL default '3600' COMMENT 'Minimum advert time in seconds ( minutes is 15*60=900)',
  `randomadvertdelay` bigint(20) NOT NULL default '3600' COMMENT 'Random delay between ads in seconds',
  `altadvertchannel` bigint(20) NOT NULL,
  `lastaltadverttime` datetime NOT NULL,
  `altadvertise` tinyint(4) NOT NULL default '0',
  `altadvertendmessage` text NOT NULL,
  `minaltadverttime` bigint(20) NOT NULL,
  `randomaltadvertdelay` bigint(20) NOT NULL,
  `newhourcolour` bigint(20) NOT NULL,
  `gmhnhmessage` tinyint(4) NOT NULL default '0' COMMENT 'Do we send the Happy New Hour message via GM?',
  `localhnhmessage` bigint(20) NOT NULL default '0' COMMENT 'Do we send the Happy New Hour message via local chat?',
  `hnhmessage` text NOT NULL COMMENT 'Happy New Hour message to broadcast',
  `playerloggedoncolor` bigint(20) NOT NULL,
  `playerloggedoffcolor` bigint(20) NOT NULL,
  `playerjoinedguildcolor` bigint(20) NOT NULL,
  `playerleftguildcolor` bigint(20) NOT NULL,
  `foodlevel` bigint(20) NOT NULL,
  `materialpointscur` bigint(20) NOT NULL,
  `materialpointsbase` bigint(20) NOT NULL,
  `etherealpointscur` bigint(20) NOT NULL,
  `etherealpointsbase` bigint(20) NOT NULL,
  `physiquecur` bigint(20) NOT NULL,
  `physiquebase` bigint(20) NOT NULL,
  `coordinationcur` bigint(20) NOT NULL,
  `coordinationbase` bigint(20) NOT NULL,
  `reasoningcur` bigint(20) NOT NULL,
  `reasoningbase` bigint(20) NOT NULL,
  `willcur` bigint(20) NOT NULL,
  `willbase` bigint(20) NOT NULL,
  `instinctcur` bigint(20) NOT NULL,
  `instinctbase` bigint(20) NOT NULL,
  `vitalitycur` bigint(20) NOT NULL,
  `vitalitybase` bigint(20) NOT NULL,
  `humannexuscur` bigint(20) NOT NULL,
  `humannexusbase` bigint(20) NOT NULL,
  `animalnexuscur` bigint(20) NOT NULL,
  `animalnexusbase` bigint(20) NOT NULL,
  `vegetalnexuscur` bigint(20) NOT NULL,
  `vegetalnexusbase` bigint(20) NOT NULL,
  `inorganicnexuscur` bigint(20) NOT NULL,
  `inorganicnexusbase` bigint(20) NOT NULL,
  `artificialnexuscur` bigint(20) NOT NULL,
  `artificialnexusbase` bigint(20) NOT NULL,
  `magicnexuscur` bigint(20) NOT NULL,
  `magicnexusbase` bigint(20) NOT NULL,
  `manufacturingskillcur` bigint(20) NOT NULL,
  `manufacturingskillbase` bigint(20) NOT NULL,
  `harvestingskillcur` bigint(20) NOT NULL,
  `harvestingskillBase` bigint(20) NOT NULL,
  `alchemyskillcur` bigint(20) NOT NULL,
  `alchemyskillbase` bigint(20) NOT NULL,
  `overallskillcur` bigint(20) NOT NULL,
  `overallskillbase` bigint(20) NOT NULL,
  `attackskillcur` bigint(20) NOT NULL,
  `attackskillbase` bigint(20) NOT NULL,
  `defenseskillcur` bigint(20) NOT NULL,
  `defenseskillbase` bigint(20) NOT NULL,
  `magicskillcur` bigint(20) NOT NULL,
  `magicskillbase` bigint(20) NOT NULL,
  `potionskillcur` bigint(20) NOT NULL,
  `potionskillbase` bigint(20) NOT NULL,
  `carrycapacitycur` bigint(20) NOT NULL,
  `carrycapacitybase` bigint(20) NOT NULL,
  `manufacturingexp` bigint(20) NOT NULL,
  `manufacturingexpnextlevel` bigint(20) NOT NULL,
  `harvestingexp` bigint(20) NOT NULL,
  `harvestingexpnextlevel` bigint(20) NOT NULL,
  `alchemyexp` bigint(20) NOT NULL,
  `alchemyexpnextlevel` bigint(20) NOT NULL,
  `overallexp` bigint(20) NOT NULL,
  `overallexpnextlevel` bigint(20) NOT NULL,
  `attackexp` bigint(20) NOT NULL,
  `attackexpnextlevel` bigint(20) NOT NULL,
  `defenseexp` bigint(20) NOT NULL,
  `defenseexpnextlevel` bigint(20) NOT NULL,
  `magicexp` bigint(20) NOT NULL,
  `magicexpnextlevel` bigint(20) NOT NULL,
  `potionexp` bigint(20) NOT NULL,
  `potionexpnextlevel` bigint(20) NOT NULL,
  `summoningskillcur` bigint(20) NOT NULL,
  `summoningskillbase` bigint(20) NOT NULL,
  `summoningexp` bigint(20) NOT NULL,
  `summoningexpnextlevel` bigint(20) NOT NULL,
  `craftingskillcur` bigint(20) NOT NULL,
  `craftingskillbase` bigint(20) NOT NULL,
  `craftingskillnextlevel` bigint(20) NOT NULL,
  `craftingexp` bigint(20) NOT NULL,
  `craftingexpnextLevel` bigint(20) NOT NULL,
  `engineeringskillcur` bigint(20) NOT NULL,
  `engineeringskillbase` bigint(20) NOT NULL,
  `engineeringskillnextlevel` bigint(20) NOT NULL,
  `engineeringexp` bigint(20) NOT NULL,
  `engineeringexpnextlevel` bigint(20) NOT NULL,
  `tailoringskillcur` bigint(20) NOT NULL,
  `tailoringskillbase` bigint(20) NOT NULL,
  `tailoringskillnextlevel` bigint(20) NOT NULL,
  `tailoringexp` bigint(20) NOT NULL,
  `tailoringexpnextlevel` bigint(20) NOT NULL,
  `researchcompleted` bigint(20) NOT NULL,
  `researching` bigint(20) NOT NULL,
  `researchtotal` bigint(20) NOT NULL,
  `connectedToServer` tinyint(1) NOT NULL,
  `launchPath` varchar(100) NOT NULL,
  `pid` bigint(20) unsigned NOT NULL default '0',
  `textlocation` varchar(100) NOT NULL default '',
  `codeversion` varchar(45) NOT NULL default '',
  `onmap` varchar(45) NOT NULL default '',
  PRIMARY KEY  (`id`),
  KEY `Index_botstatus` (`connectedToServer`),
  KEY `Index_botname` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

--
-- Table structure for table `botsettings`
--

DROP TABLE IF EXISTS `botsettings`;
CREATE TABLE `botsettings` (
  `id` int(10) unsigned NOT NULL auto_increment,
  `botid` int(10) unsigned NOT NULL,
  `istradebot` tinyint(1) unsigned NOT NULL,
  `tradetimout` tinyint(1) unsigned NOT NULL,
  `guildfeedgossip` tinyint(1) unsigned NOT NULL,
  `debuggingtofile` tinyint(1) unsigned NOT NULL,
  `eventlogtofile` tinyint(1) unsigned NOT NULL,
  `chatlogtofile` tinyint(1) unsigned NOT NULL,
  `pmlogtofile` tinyint(1) unsigned NOT NULL,
  `enabledebugging` tinyint(1) unsigned NOT NULL,
  `loggotpm` tinyint(1) unsigned NOT NULL,
  `logsendpm` tinyint(1) unsigned NOT NULL,
  `logglobalmessage` tinyint(1) unsigned NOT NULL,
  `loggossipfeed` tinyint(1) unsigned NOT NULL,
  `password` varchar(45) NOT NULL,
  `autoreconnect` tinyint(1) unsigned NOT NULL,
  PRIMARY KEY  (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Table structure for table `botsonline`
--

DROP TABLE IF EXISTS `botsonline`;
CREATE TABLE `botsonline` (
  `name` varchar(45) NOT NULL,
  PRIMARY KEY  (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Table structure for table `commands`
--

DROP TABLE IF EXISTS `commands`;
CREATE TABLE `commands` (
  `id` bigint(20) NOT NULL auto_increment,
  `name` varchar(45) NOT NULL,
  `requriedrank` bigint(20) unsigned NOT NULL,
  `description` text NOT NULL,
  `disabled` tinyint(4) NOT NULL default '0',
  `botid` bigint(20) NOT NULL,
  PRIMARY KEY  (`id`),
  UNIQUE KEY `Index_cmdunique` (`botid`,`name`)
) ENGINE=MyISAM AUTO_INCREMENT=1425 DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

--
-- Table structure for table `globalsettings`
--

DROP TABLE IF EXISTS `globalsettings`;
CREATE TABLE `globalsettings` (
  `id` int(10) unsigned NOT NULL auto_increment,
  `name` varchar(45) NOT NULL,
  `serverhostname` varchar(45) NOT NULL,
  `serverport` int(10) unsigned NOT NULL,
  `currentversion` varchar(45) NOT NULL,
  PRIMARY KEY  (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;

--
-- Table structure for table `guilds`
--

DROP TABLE IF EXISTS `guilds`;
CREATE TABLE `guilds` (
  `id` bigint(20) NOT NULL auto_increment,
  `guildname` text NOT NULL,
  `rank` bigint(20) NOT NULL,
  `botid` bigint(20) NOT NULL,
  `notes` text NOT NULL,
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=52 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;

--
-- Table structure for table `inventory`
--

DROP TABLE IF EXISTS `inventory`;
CREATE TABLE `inventory` (
  `id` bigint(20) NOT NULL auto_increment,
  `pos` bigint(20) NOT NULL,
  `quantity` bigint(20) NOT NULL,
  `knownitemsid` bigint(20) NOT NULL,
  `botid` bigint(20) NOT NULL,
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=458598 DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

--
-- Table structure for table `knownitems`
--

DROP TABLE IF EXISTS `knownitems`;
CREATE TABLE `knownitems` (
  `id` bigint(20) NOT NULL auto_increment,
  `name` char(255) NOT NULL,
  `description` text NOT NULL,
  `imageid` bigint(20) NOT NULL,
  `weight` bigint(20) NOT NULL,
  `is_resource` int(11) default NULL,
  `is_reagent` int(11) NOT NULL,
  `is_stackable` int(11) NOT NULL,
  `use_with_inventory` int(11) NOT NULL,
  PRIMARY KEY  (`id`),
  UNIQUE KEY `Index_3` (`name`),
  KEY `Index_2` USING BTREE (`imageid`,`name`)
) ENGINE=MyISAM AUTO_INCREMENT=485 DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

--
-- Table structure for table `otheradverts`
--

DROP TABLE IF EXISTS `otheradverts`;
CREATE TABLE `otheradverts` (
  `id` bigint(20) NOT NULL auto_increment,
  `botid` bigint(20) NOT NULL,
  `channel` bigint(20) NOT NULL default '0',
  `text` text NOT NULL,
  `disabled` tinyint(4) NOT NULL,
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=41 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;

--
-- Table structure for table `players`
--

DROP TABLE IF EXISTS `players`;
CREATE TABLE `players` (
  `id` bigint(20) NOT NULL auto_increment,
  `name` varchar(45) NOT NULL,
  `guild` text NOT NULL,
  `lastseen` timestamp NOT NULL default CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP,
  `lastseenby` text NOT NULL,
  PRIMARY KEY  (`id`),
  KEY `Index_playername` (`name`)
) ENGINE=MyISAM AUTO_INCREMENT=9932 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;

--
-- Table structure for table `price_teir`
--

DROP TABLE IF EXISTS `price_teir`;
CREATE TABLE `price_teir` (
  `id` int(10) unsigned NOT NULL auto_increment,
  `Description` varchar(45) NOT NULL,
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=5 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;

--
-- Table structure for table `pricelist`
--

DROP TABLE IF EXISTS `pricelist`;
CREATE TABLE `pricelist` (
  `id` bigint(20) NOT NULL auto_increment,
  `knownitemsid` bigint(20) NOT NULL,
  `pricesale` double NOT NULL default '0',
  `pricesalemembers` double NOT NULL default '0',
  `pricepurchase` double NOT NULL default '0',
  `pricepurchasemembers` double NOT NULL,
  `botid` bigint(20) NOT NULL,
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=346 DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

--
-- Table structure for table `prices`
--

DROP TABLE IF EXISTS `prices`;
CREATE TABLE `prices` (
  `id` int(10) unsigned NOT NULL auto_increment,
  `pricelist_id` int(10) unsigned NOT NULL,
  `price_teir_id` int(10) unsigned NOT NULL,
  `price` double NOT NULL,
  `botid` tinyint(3) unsigned NOT NULL,
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=5 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;

--
-- Table structure for table `project`
--

DROP TABLE IF EXISTS `project`;
CREATE TABLE `project` (
  `id` int(10) unsigned NOT NULL auto_increment,
  `description` varchar(45) NOT NULL default 'You forgot your description!',
  `owner` varchar(45) NOT NULL,
  `password` varchar(45) default NULL,
  PRIMARY KEY  (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Table structure for table `reservedamount`
--

DROP TABLE IF EXISTS `reservedamount`;
CREATE TABLE `reservedamount` (
  `id` int(10) unsigned NOT NULL auto_increment,
  `knownitemsid` int(10) unsigned NOT NULL,
  `quantity` int(10) unsigned NOT NULL,
  `reservedBy` varchar(45) NOT NULL,
  `botid` int(10) unsigned NOT NULL,
  `projectid` int(11) NOT NULL,
  PRIMARY KEY  (`id`),
  UNIQUE KEY `IdxReservedUnique` USING BTREE (`botid`,`knownitemsid`,`reservedBy`,`projectid`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Table structure for table `sellingitems`
--

DROP TABLE IF EXISTS `sellingitems`;
CREATE TABLE `sellingitems` (
  `id` bigint(20) NOT NULL auto_increment,
  `pricelistid` bigint(20) NOT NULL,
  `announce` tinyint(3) unsigned NOT NULL default '0',
  `botid` bigint(20) NOT NULL,
  `disabled` tinyint(3) unsigned NOT NULL default '0',
  PRIMARY KEY  (`id`),
  UNIQUE KEY `Index_sellingunique` (`botid`,`pricelistid`)
) ENGINE=MyISAM AUTO_INCREMENT=328 DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

--
-- Table structure for table `storage`
--

DROP TABLE IF EXISTS `storage`;
CREATE TABLE `storage` (
  `id` int(10) unsigned NOT NULL auto_increment,
  `name` varchar(45) NOT NULL,
  `knownitemsid` int(10) unsigned NOT NULL,
  `imageid` int(10) unsigned NOT NULL,
  `quantity` int(10) unsigned NOT NULL,
  `botid` int(10) unsigned NOT NULL,
  `categorynum` int(11) NOT NULL,
  PRIMARY KEY  (`id`),
  UNIQUE KEY `UniqueStorage` USING BTREE (`knownitemsid`,`botid`,`categorynum`)
) ENGINE=MyISAM AUTO_INCREMENT=90900 DEFAULT CHARSET=latin1;

--
-- Table structure for table `storagecategory`
--

DROP TABLE IF EXISTS `storagecategory`;
CREATE TABLE `storagecategory` (
  `id` int(11) NOT NULL auto_increment,
  `name` varchar(45) NOT NULL,
  `botid` int(10) unsigned NOT NULL,
  PRIMARY KEY  (`id`,`name`,`botid`)
) ENGINE=MyISAM AUTO_INCREMENT=119 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;

--
-- Table structure for table `storagecolors`
--

DROP TABLE IF EXISTS `storagecolors`;
CREATE TABLE `storagecolors` (
  `id` int(10) unsigned NOT NULL auto_increment,
  `category` int(11) NOT NULL,
  `textcolor` varchar(20) NOT NULL,
  `bgcolor` varchar(20) NOT NULL,
  `botid` bigint(20) unsigned NOT NULL,
  PRIMARY KEY  USING BTREE (`id`,`category`,`botid`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;

--
-- Table structure for table `systemmessage`
--

DROP TABLE IF EXISTS `systemmessage`;
CREATE TABLE `systemmessage` (
  `id` int(10) unsigned NOT NULL auto_increment,
  `startdate` timestamp NOT NULL default CURRENT_TIMESTAMP,
  `enddate` timestamp NOT NULL default '0000-00-00 00:00:00',
  `message` varchar(150) NOT NULL default 'You forgot your message.',
  `botid` tinyint(3) unsigned NOT NULL default '0',
  PRIMARY KEY  (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;

--
-- Table structure for table `tempcommands`
--

DROP TABLE IF EXISTS `tempcommands`;
CREATE TABLE `tempcommands` (
  `id` bigint(20) NOT NULL default '0',
  `name` text character set utf8 NOT NULL,
  `requriedrank` bigint(20) NOT NULL,
  `description` text character set utf8 NOT NULL,
  `disabled` tinyint(4) NOT NULL default '0',
  `botid` bigint(20) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Table structure for table `textcommands`
--

DROP TABLE IF EXISTS `textcommands`;
CREATE TABLE `textcommands` (
  `id` bigint(20) NOT NULL auto_increment,
  `command` text NOT NULL COMMENT 'eg #eltc',
  `helptext` text NOT NULL COMMENT 'eg "show what I am buying"',
  `text` text NOT NULL COMMENT 'eg "blah blah blah" lines are 0d/0a broken',
  `disabled` tinyint(4) NOT NULL default '0' COMMENT 'is command disabled?',
  `rank` int(11) NOT NULL COMMENT 'rank required to use command',
  `botid` int(11) NOT NULL COMMENT 'bot id',
  `sendtogm` tinyint(4) NOT NULL default '0' COMMENT 'Should we send this command to #GM chat?',
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=6 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;

--
-- Table structure for table `tradelog`
--

DROP TABLE IF EXISTS `tradelog`;
CREATE TABLE `tradelog` (
  `id` int(11) NOT NULL auto_increment,
  `knownitemsid` bigint(20) NOT NULL,
  `quantity` int(11) NOT NULL,
  `price` double NOT NULL,
  `username` text NOT NULL,
  `tradetime` datetime NOT NULL,
  `action` text NOT NULL,
  `payedout` int(11) NOT NULL,
  `botid` bigint(20) NOT NULL,
  PRIMARY KEY  (`id`),
  KEY `Index_3` (`knownitemsid`),
  KEY `Index_2` USING BTREE (`botid`,`knownitemsid`)
) ENGINE=MyISAM AUTO_INCREMENT=28128 DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

--
-- Table structure for table `user_price`
--

DROP TABLE IF EXISTS `user_price`;
CREATE TABLE `user_price` (
  `id` int(10) unsigned NOT NULL auto_increment,
  `prices_id` int(10) unsigned NOT NULL,
  `users_id` int(10) unsigned NOT NULL,
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=2 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
CREATE TABLE `users` (
  `id` bigint(20) NOT NULL auto_increment,
  `name` text NOT NULL,
  `rank` bigint(20) NOT NULL,
  `password` blob,
  `isguildmember` int(11) NOT NULL default '0',
  `guildrank` int(11) NOT NULL default '0',
  `istrademember` int(11) NOT NULL default '0',
  `greeting` varchar(90) default '',
  `botid` bigint(20) NOT NULL,
  `banreason` varchar(90) default '',
  `is_owner` tinyint(3) unsigned NOT NULL default '0',
  `may_launch` tinyint(3) unsigned NOT NULL default '0',
  `view_only` tinyint(3) unsigned NOT NULL default '0',
  PRIMARY KEY  (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

--
-- Table structure for table `wanteditems`
--

DROP TABLE IF EXISTS `wanteditems`;
CREATE TABLE `wanteditems` (
  `id` bigint(20) NOT NULL auto_increment,
  `pricelistid` bigint(20) NOT NULL,
  `maxquantity` bigint(20) NOT NULL,
  `announce` tinyint(3) unsigned NOT NULL default '0',
  `botid` bigint(20) NOT NULL,
  `disabled` tinyint(3) unsigned NOT NULL default '0',
  PRIMARY KEY  (`id`),
  UNIQUE KEY `Index_wanted_unique` (`botid`,`pricelistid`)
) ENGINE=MyISAM AUTO_INCREMENT=309 DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC;

--
-- Table structure for table `weblog`
--

DROP TABLE IF EXISTS `weblog`;
CREATE TABLE `weblog` (
  `id` bigint(20) NOT NULL auto_increment,
  `date` text NOT NULL,
  `botid` bigint(20) NOT NULL,
  `username` text NOT NULL,
  `action` text NOT NULL,
  `webaddress` text NOT NULL,
  PRIMARY KEY  (`id`)
) ENGINE=MyISAM AUTO_INCREMENT=2720 DEFAULT CHARSET=latin1 ROW_FORMAT=DYNAMIC;

--
-- Table structure for table `weboptions`
--

DROP TABLE IF EXISTS `weboptions`;
CREATE TABLE `weboptions` (
  `botid` tinyint(3) unsigned NOT NULL,
  `mouseheader` tinyint(1) unsigned NOT NULL default '1',
  `mouseother` tinyint(1) unsigned NOT NULL default '1',
  `showpublicsaleprice` tinyint(1) unsigned NOT NULL default '1',
  `showpublicpurchaseprice` tinyint(1) unsigned NOT NULL default '1',
  PRIMARY KEY  (`botid`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2008-09-12 12:47:49
