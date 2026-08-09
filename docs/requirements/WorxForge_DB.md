# Entity Relationship Diagram (ERD)

This document is a direct Markdown representation of the provided ERD.

---

## Relationships

### Authentication  User

- `USER` 1 → Many `AUTH_SESSION`
  - A user can have multiple authentication sessions.

---

### Core Asset Ownership

- `USER` 1 → Many `ASSET`
  - A user owns multiple assets.

- `ASSET_TYPE` 1 → Many `ASSET`
  - Each asset belongs to one asset type.

- `DOMAIN` 1 → Many `ASSET`
  - Each asset belongs to one domain.

- `USE_CASE` 1 → Many `ASSET`
  - Each asset belongs to one use case.

---

### Asset Classification (Many-to-Many)

- `ASSET` Many ↔ Many `TAG`
  - Junction Table `ASSET_TAG`

- `ASSET` Many ↔ Many `TOOL`
  - Junction Table `ASSET_TOOL`

---

### Collaboration

- `ASSET` 1 → Many `COMMENT`
  - Assets can have multiple comments.

- `USER` 1 → Many `COMMENT`
  - Users write comments.

- `COMMENT` 1 → Many `COMMENT`
  - Supports nested replies using `parent_comment_id`.

- `ASSET` 1 → Many `REACTION`
  - Assets receive reactions.

- `USER` 1 → Many `REACTION`
  - Users react to assets.

---

### Usage & Library

- `USER` 1 → Many `ASSET_VIEW`
  - Tracks viewing history.

- `ASSET` 1 → Many `ASSET_VIEW`
  - Tracks users who viewed the asset.

- `USER` 1 → Many `BOOKMARK`
  - Users bookmark assets.

- `ASSET` 1 → Many `BOOKMARK`
  - Assets can be bookmarked.

---

### Moderation

- `USER` 1 → Many `REPORT`
  - Users file reports.

- `USER` 1 → Many `REPORT`
  - Users may resolve reports.

- `REPORT` 1 → 0..1 `REPORT_ASSET`
  - Report targets an asset.

- `ASSET` 1 → Many `REPORT_ASSET`

---

### Notifications

- `USER` 1 → Many `NOTIFICATION`
  - Recipient relationship.

- `USER` 1 → Many `NOTIFICATION`
  - Actor relationship.

- `ASSET` 1 → Many `NOTIFICATION`

- `COMMENT` 1 → Many `NOTIFICATION`

- `REPORT` 1 → Many `NOTIFICATION`

---

# Tables

## USER

 Column  Type  Key 
--------------------
 user_id  uuid  PK 
 username  string  
 display_name  string  
 email  string  
 email_verified  boolean  
 role  string  
 google_id  string  
 avatar_url  string  
 created_at  datetime  
 updated_at  datetime  
 deleted_at  datetime  

---

## AUTH_SESSION

 Column  Type  Key 
--------------------
 session_id  uuid  PK 
 user_id  uuid  FK 
 token_hash  string  
 issued_at  datetime  
 expires_at  datetime  
 revoked_at  datetime  

---

## ASSET_TYPE

 Column  Type  Key 
--------------------
 asset_type_id  uuid  PK 
 name  string  UK 

---

## DOMAIN

 Column  Type  Key 
--------------------
 domain_id  uuid  PK 
 name  string  UK 

---

## USE_CASE

 Column  Type  Key 
--------------------
 use_case_id  uuid  PK 
 name  string  UK 

---

## ASSET

 Column  Type  Key 
--------------------
 asset_id  uuid  PK 
 author_id  uuid  FK 
 title  string  
 description  string  
 markdown_content  string  
 asset_type_id  uuid  FK 
 domain_id  uuid  FK 
 use_case_id  uuid  FK 
 created_at  datetime  
 updated_at  datetime  
 deleted_at  datetime  nullable

---

## TOOL

 Column  Type  Key 
--------------------
 tool_id  uuid  PK 
 name  string  UK 

---

## ASSET_TOOL

 Column  Type  Key 
--------------------
 asset_id  uuid  PK, FK 
 tool_id  uuid  PK, FK 

---

## TAG

 Column  Type  Key 
--------------------
 tag_id  uuid  PK 
 name  string  UK 

---

## ASSET_TAG

 Column  Type  Key 
--------------------
 asset_id  uuid  PK, FK 
 tag_id  uuid  PK, FK 

---

## COMMENT

 Column  Type  Key 
--------------------
 comment_id  uuid  PK 
 asset_id  uuid  FK 
 user_id  uuid  FK 
 parent_comment_id  uuid  FK 
 content  text  
 created_at  datetime  
 updated_at  datetime  nullable
 deleted_at  datetime  nullable

---

## REACTION

 Column  Type  Key 
--------------------
 asset_id  uuid  PK, FK 
 user_id  uuid  PK, FK 
 reaction_type  string  

---

## ASSET_VIEW

 Column  Type  Key 
--------------------
 user_id  uuid  PK, FK 
 asset_id  uuid  PK, FK 
 last_viewed_at  datetime  
 view_count  int  

---

## BOOKMARK

 Column  Type  Key 
--------------------
 user_id  uuid  PK, FK 
 asset_id  uuid  PK, FK 

---

## REPORT

 Column  Type  Key 
--------------------
 report_id  uuid  PK 
 reporter_user_id  uuid  FK 
 resolved_by  uuid  FK 
 reason  string  
 status  enum  
 created_at  datetime  
 resolved_at  datetime  

---

## REPORT_ASSET

 Column  Type  Key 
--------------------
 report_id  uuid  PK, FK 
 asset_id  uuid  FK 

---

## NOTIFICATION

 Column  Type  Key 
--------------------
 notification_id  uuid  PK 
 recipient_user_id  uuid  FK 
 actor_user_id  uuid  FK 
 type  string  
 message  string  
 asset_id  uuid  FK 
 comment_id  uuid  FK 
 report_id  uuid  FK 
 is_read  boolean  
 created_at  datetime  
 read_at  datetime  