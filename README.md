# Template Vault

A simple proof of concept that processes a template
file, pulls the needed values out of Vault, then
writes the result to a new file.

## Using Template Vault

### Installation

Before being used Template Vault must be installed.
It is published as a DotNet tool and can easily be installed via nuget like so:

`$ dotnet tool install --global TemplateVault`

This installation only needs to be done once.

### Running

Run using the input template file as the first
argument:

`$ VaultTemplate appsettings.local.json.tmpl --auth okta`

The template file should end in the extension
`.tmpl` or `.tpl`

The output file will be the name of the input file
excluding this extension.

## Templates

### Vault Root Url

Templates must include a comment containing the root
Vault url to use like so:

`// {{VAULTROOT: https://vault.example.com}}`

The Vault root url may include a default path to
search for secrets like so:

`// {{VAULTROOT: https://vault.example.com/secrets/default}}`

### Secret Paths

Secrets are included in the template inside pairs of
double curly braces and may be absolute or relative:

`{{/secrets/default/secret1}}`

`{{secret1}}`

Assuming the vault root is set to
`https://vault.example.com/secrets/default`
both of the above would resolve to the same secret.

***Note that common relative folder operators
`.` (current folder) and `..` (up one directory)
are not supported.***

### Example

#### Input Template

```json
// {{VAULTROOT: https://vault.example.com/secrets/default}}
{
    "key1":"{{/secrets/default/secret1}}",
    "key2":"{{secret1}}"
}
```

#### Output File

```json
// {{VAULTROOT: https://vault.example.com/secrets/default}}
{
    "key1":"value1",
    "key2":"value1"
}
```

## Note on Secret Paths

A secret in Vault is technically identified by three
pieces of information: the mount point, the path, and
the secret key/name. Template Vault abstracts this
away so that the mount point is prepended to the
beginning of the path and the key/name appended to
the end of the path. The mount point and key/name must
always be included in the path. The mount point can
be provided by Vault root for relative secret paths.
But it must always be included in the beginning of
absolute secret paths.

For example, given a mount point `secrets`, a path
`default/my-secrets`, and a key/name `secret1`  the
full path must be `secrets/default/my-secrets/secret1`.
All of the below would resolve to this secret.

| Vault Root                                           | Path                                |
|------------------------------------------------------|-------------------------------------|
| https://vault.example.com                            | secrets/default/my-secrets/secret1  |
| https://vault.example.com/secrets/default/my-secrets | secret1                             |
| https://vault.example.com/secrets/default/my-secrets | /secrets/default/my-secrets/secret1 |

Note how in the last example the path is defined as an
absolute path so it ignored the path included in the
Vault root.

## Auth

### Auth Types

Currently the following auth types are supported:

| Command Line       | Description                                          |
|--------------------|------------------------------------------------------|
| --auth approle     | AppRole authentication                               |
| --auth azure       | Azure JWT authorization                              |
| --auth github      | GitHub private token authentication                  |
| --auth gcp         | Google Cloud JWT authentication                      |
| --auth jwt         | JWT authentication                                   |
| --auth kerbos      | Kerbos username and password authentication          |
| --auth kubernetes  | Kubernetes JWT authentication                        |
| --auth ldap        | LDAP username and password authentication            |
| --auth okta        | OKTA username and password authentication            |
| --auth radius      | RADIUS username and password authentication          |
| --auth token       | Vault Token authentication                           |
| --auth userpass    | Vault username and password authentication (default) |

Vault Username/Password authentication is the default, if
no `--auth` parameter is supplied it will be used.

### Non-Standard Auth Mounts

In some cases the auth backend is mounted in Vault in a
non-standard location. If this is the case the
`--auth-mount` option can be provided with the location
of the auth mount point to use.