\connect auth_db

create temp table tmp_test_users (user_id uuid);
\copy tmp_test_users from 'tests/.artifacts/test-users.txt'

delete from "RefreshTokens" where "UserId" in (select user_id from tmp_test_users);
delete from "AspNetUserTokens" where "UserId" in (select user_id from tmp_test_users);
delete from "AspNetUserRoles" where "UserId" in (select user_id from tmp_test_users);
delete from "AspNetUserLogins" where "UserId" in (select user_id from tmp_test_users);
delete from "AspNetUserClaims" where "UserId" in (select user_id from tmp_test_users);
delete from "AspNetUsers" where "Id" in (select user_id from tmp_test_users);

drop table tmp_test_users;

\connect userprofile_db

create temp table tmp_test_users (user_id uuid);
\copy tmp_test_users from 'tests/.artifacts/test-users.txt'

delete from "AdditionalInfo" where "UserId" in (select user_id from tmp_test_users);
delete from "Profiles" where "UserId" in (select user_id from tmp_test_users);

drop table tmp_test_users;
