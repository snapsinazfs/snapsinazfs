namespace SnapsInAZfs.Interop.Zfs.Enums;

public enum LibZfsError
{
    EZFS_SUCCESS = 0,           /* no error -- success */
    EZFS_NOMEM = 2000,          /* out of memory */
    EZFS_BADPROP,               /* invalid property value */
    EZFS_PROPREADONLY,          /* cannot set readonly property */
    EZFS_PROPTYPE,              /* property does not apply to dataset type */
    EZFS_PROPNONINHERIT,        /* property is not inheritable */
    EZFS_PROPSPACE,             /* bad quota or reservation */
    EZFS_BADTYPE,               /* dataset is not of appropriate type */
    EZFS_BUSY,                  /* pool or dataset is busy */
    EZFS_EXISTS,                /* pool or dataset already exists */
    EZFS_NOENT,                 /* no such pool or dataset */
    EZFS_BADSTREAM,             /* bad backup stream */
    EZFS_DSREADONLY,            /* dataset is readonly */
    EZFS_VOLTOOBIG,             /* volume is too large for 32-bit system */
    EZFS_INVALIDNAME,           /* invalid dataset name */
    EZFS_BADRESTORE,            /* unable to restore to destination */
    EZFS_BADBACKUP,             /* backup failed */
    EZFS_BADTARGET,             /* bad attach/detach/replace target */
    EZFS_NODEVICE,              /* no such device in pool */
    EZFS_BADDEV,                /* invalid device to add */
    EZFS_NOREPLICAS,            /* no valid replicas */
    EZFS_RESILVERING,           /* resilvering (healing reconstruction) */
    EZFS_BADVERSION,            /* unsupported version */
    EZFS_POOLUNAVAIL,           /* pool is currently unavailable */
    EZFS_DEVOVERFLOW,           /* too many devices in one vdev */
    EZFS_BADPATH,               /* must be an absolute path */
    EZFS_CROSSTARGET,           /* rename or clone across pool or dataset */
    EZFS_ZONED,                 /* used improperly in local zone */
    EZFS_MOUNTFAILED,           /* failed to mount dataset */
    EZFS_UMOUNTFAILED,          /* failed to unmount dataset */
    EZFS_UNSHARENFSFAILED,      /* failed to unshare over nfs */
    EZFS_SHARENFSFAILED,        /* failed to share over nfs */
    EZFS_PERM,                  /* permission denied */
    EZFS_NOSPC,                 /* out of space */
    EZFS_FAULT,                 /* bad address */
    EZFS_IO,                    /* I/O error */
    EZFS_INTR,                  /* signal received */
    EZFS_ISSPARE,               /* device is a hot spare */
    EZFS_INVALCONFIG,           /* invalid vdev configuration */
    EZFS_RECURSIVE,             /* recursive dependency */
    EZFS_NOHISTORY,             /* no history object */
    EZFS_POOLPROPS,             /* couldn't retrieve pool props */
    EZFS_POOL_NOTSUP,           /* ops not supported for this type of pool */
    EZFS_POOL_INVALARG,         /* invalid argument for this pool operation */
    EZFS_NAMETOOLONG,           /* dataset name is too long */
    EZFS_OPENFAILED,            /* open of device failed */
    EZFS_NOCAP,                 /* couldn't get capacity */
    EZFS_LABELFAILED,           /* write of label failed */
    EZFS_BADWHO,                /* invalid permission who */
    EZFS_BADPERM,               /* invalid permission */
    EZFS_BADPERMSET,            /* invalid permission set name */
    EZFS_NODELEGATION,          /* delegated administration is disabled */
    EZFS_UNSHARESMBFAILED,      /* failed to unshare over smb */
    EZFS_SHARESMBFAILED,        /* failed to share over smb */
    EZFS_BADCACHE,              /* bad cache file */
    EZFS_ISL2CACHE,             /* device is for the level 2 ARC */
    EZFS_VDEVNOTSUP,            /* unsupported vdev type */
    EZFS_NOTSUP,                /* ops not supported on this dataset */
    EZFS_ACTIVE_SPARE,          /* pool has active shared spare devices */
    EZFS_UNPLAYED_LOGS,         /* log device has unplayed logs */
    EZFS_REFTAG_RELE,           /* snapshot release: tag not found */
    EZFS_REFTAG_HOLD,           /* snapshot hold: tag already exists */
    EZFS_TAGTOOLONG,            /* snapshot hold/rele: tag too long */
    EZFS_PIPEFAILED,            /* pipe create failed */
    EZFS_THREADCREATEFAILED,    /* thread create failed */
    EZFS_POSTSPLIT_ONLINE,      /* onlining a disk after splitting it */
    EZFS_SCRUBBING,             /* currently scrubbing */
    EZFS_NO_SCRUB,              /* no active scrub */
    EZFS_DIFF,                  /* general failure of zfs diff */
    EZFS_DIFFDATA,              /* bad zfs diff data */
    EZFS_POOLREADONLY,          /* pool is in read-only mode */
    EZFS_SCRUB_PAUSED,          /* scrub currently paused */
    EZFS_ACTIVE_POOL,           /* pool is imported on a different system */
    EZFS_CRYPTOFAILED,          /* failed to setup encryption */
    EZFS_NO_PENDING,            /* cannot cancel, no operation is pending */
    EZFS_CHECKPOINT_EXISTS,     /* checkpoint exists */
    EZFS_DISCARDING_CHECKPOINT, /* currently discarding a checkpoint */
    EZFS_NO_CHECKPOINT,         /* pool has no checkpoint */
    EZFS_DEVRM_IN_PROGRESS,     /* a device is currently being removed */
    EZFS_VDEV_TOO_BIG,          /* a device is too big to be used */
    EZFS_IOC_NOTSUPPORTED,      /* operation not supported by zfs module */
    EZFS_TOOMANY,               /* argument list too long */
    EZFS_INITIALIZING,          /* currently initializing */
    EZFS_NO_INITIALIZE,         /* no active initialize */
    EZFS_WRONG_PARENT,          /* invalid parent dataset (e.g ZVOL) */
    EZFS_TRIMMING,              /* currently trimming */
    EZFS_NO_TRIM,               /* no active trim */
    EZFS_TRIM_NOTSUP,           /* device does not support trim */
    EZFS_NO_RESILVER_DEFER,     /* pool doesn't support resilver_defer */
    EZFS_EXPORT_IN_PROGRESS,    /* currently exporting the pool */
    EZFS_REBUILDING,            /* resilvering (sequential reconstrution) */
    EZFS_VDEV_NOTSUP,           /* ops not supported for this type of vdev */
    EZFS_NOT_USER_NAMESPACE,    /* a file is not a user namespace */
    EZFS_CKSUM,                 /* insufficient replicas */
    EZFS_RESUME_EXISTS,         /* Resume on existing dataset without force */
    EZFS_UNKNOWN
}
