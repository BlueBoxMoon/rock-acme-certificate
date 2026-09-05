using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using Rock.Enums.Cms;
using Rock.Plugin;

namespace com.blueboxmoon.AcmeCertificate.Migrations
{
    public abstract class ExtendedMigration : Migration
    {
        /// <summary>
        /// Converts an existing WebForms block type (and its block instances) into an
        /// Obsidian block type in place. The BlockType row is preserved, so block
        /// instances, attributes, and attribute values remain intact; only the Guid,
        /// Path, and EntityTypeId are updated.
        /// </summary>
        /// <param name="webFormsBlockTypeGuid">The Guid of the existing WebForms block type.</param>
        /// <param name="obsidianBlockTypeGuid">The Guid to assign to the converted Obsidian block type.</param>
        /// <param name="obsidianBlockEntityTypeGuid">The Guid of the EntityType that backs the Obsidian block class.</param>
        protected void ConvertWebFormsBlockToObsidian( Guid webFormsBlockTypeGuid, Guid obsidianBlockTypeGuid, Guid obsidianBlockEntityTypeGuid )
        {
            Sql( @"
DECLARE @ObsidianBlockEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = @ObsidianBlockEntityTypeGuid)

-- Delete any manually created instances of the Obsidian block and block type.
DELETE [B]
FROM [Block] AS [B]
INNER JOIN [BlockType] AS [BT] ON [BT].[Id] = [B].[BlockTypeId]
WHERE [BT].[Guid] = @ObsidianBlockTypeGuid

DELETE FROM [BlockType] WHERE [Guid] = @ObsidianBlockTypeGuid

-- Convert the WebForms block to Obsidian.
UPDATE [BlockType] SET
    [Path] = NULL,
    [Guid] = @ObsidianBlockTypeGuid,
    [EntityTypeId] = @ObsidianBlockEntityTypeId,
    [SiteTypeFlags] = @SiteType
WHERE [Guid] = @WebFormsBlockTypeGuid
", new Dictionary<string, object>
            {
                ["SiteType"] = ( int ) SiteTypeFlags.Web,
                ["WebFormsBlockTypeGuid"] = webFormsBlockTypeGuid,
                ["ObsidianBlockTypeGuid"] = obsidianBlockTypeGuid,
                ["ObsidianBlockEntityTypeGuid"] = obsidianBlockEntityTypeGuid
            } );
        }

        /// <summary>
        /// Reverts an Obsidian block type back to a WebForms block type in place.
        /// The BlockType row is preserved; only the Guid, Path, and EntityTypeId change.
        /// </summary>
        /// <param name="obsidianBlockTypeGuid">The Guid of the existing Obsidian block type.</param>
        /// <param name="webFormsBlockTypeGuid">The Guid to restore for the WebForms block type.</param>
        /// <param name="webFormsPath">The relative path of the WebForms .ascx control.</param>
        protected void ConvertObsidianBlockToWebForms( Guid obsidianBlockTypeGuid, Guid webFormsBlockTypeGuid, string webFormsPath )
        {
            Sql( @"
-- Delete any manually created instances of the WebForms block and block type.
DELETE [B]
FROM [Block] AS [B]
INNER JOIN [BlockType] AS [BT] ON [BT].[Id] = [B].[BlockTypeId]
WHERE [BT].[Guid] = @WebFormsBlockTypeGuid

DELETE FROM [BlockType] WHERE [Guid] = @WebFormsBlockTypeGuid

-- Convert the Obsidian block back to WebForms.
UPDATE [BlockType] SET
    [Path] = @WebFormsPath,
    [Guid] = @WebFormsBlockTypeGuid,
    [EntityTypeId] = NULL,
    [SiteTypeFlags] = 0
WHERE [Guid] = @ObsidianBlockTypeGuid
", new Dictionary<string, object>
            {
                ["WebFormsBlockTypeGuid"] = webFormsBlockTypeGuid,
                ["ObsidianBlockTypeGuid"] = obsidianBlockTypeGuid,
                ["WebFormsPath"] = webFormsPath
            } );
        }

        protected void Sql( string sql, Dictionary<string, object> parameters = null )
        {
            if ( SqlConnection != null || SqlTransaction != null )
            {
                using ( SqlCommand sqlCommand = new SqlCommand( sql, SqlConnection, SqlTransaction ) )
                {
                    sqlCommand.CommandType = CommandType.Text;
                    if ( parameters != null )
                    {
                        foreach ( var key in parameters.Keys )
                        {
                            sqlCommand.Parameters.AddWithValue( key, parameters[key] );
                        }
                    }
                    sqlCommand.ExecuteNonQuery();
                }
            }
            else
            {
                throw new NullReferenceException( "The Plugin Migration requires valid SqlConnection and SqlTransaction values when executing SQL" );
            }
        }

        protected object SqlScalar( string sql, Dictionary<string, object> parameters = null )
        {
            if ( SqlConnection != null || SqlTransaction != null )
            {
                using ( SqlCommand sqlCommand = new SqlCommand( sql, SqlConnection, SqlTransaction ) )
                {
                    sqlCommand.CommandType = CommandType.Text;
                    if ( parameters != null )
                    {
                        foreach ( var key in parameters.Keys )
                        {
                            sqlCommand.Parameters.AddWithValue( key, parameters[key] );
                        }
                    }
                    return sqlCommand.ExecuteScalar();
                }
            }
            else
            {
                throw new NullReferenceException( "The Plugin Migration requires valid SqlConnection and SqlTransaction values when executing SQL" );
            }
        }

        protected void Sql( string sql, string key, object value )
        {
            Sql( sql, new Dictionary<string, object> { { key, value } } );
        }

        protected object SqlScalar( string sql, string key, object value )
        {
            return SqlScalar( sql, new Dictionary<string, object> { { key, value } } );
        }
    }
}
