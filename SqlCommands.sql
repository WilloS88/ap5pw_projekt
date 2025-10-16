


CREATE TABLE IF NOT EXISTS `Roles` (
  `Id`   INT NOT NULL AUTO_INCREMENT,
  `Name` VARCHAR(100) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UX_Roles_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


CREATE TABLE IF NOT EXISTS `Users` (
  `Id`       INT NOT NULL AUTO_INCREMENT,
  `Username` VARCHAR(255) NOT NULL,
  `Password` VARCHAR(255) NOT NULL,
  `RoleId`   INT NOT NULL,
  PRIMARY KEY (`Id`),

  UNIQUE KEY `UX_Users_Username` (`Username`),
  KEY `IX_Users_RoleId` (`RoleId`),

  CONSTRAINT `FK_Users_Roles_RoleId`
    FOREIGN KEY (`RoleId`) REFERENCES `Roles`(`Id`)
    ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


INSERT INTO `Roles` (`Name`) VALUES ('Admin'), ('User');