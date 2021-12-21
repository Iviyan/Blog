drop database blog;
drop role blog_server;

create ROLE blog_server with
	LOGIN
	PASSWORD '12345';

create database Blog
    with 
    OWNER = blog_server;
	
ALTER SCHEMA public OWNER TO blog_server;
grant all on all tables in schema public to blog_server;
grant all on all sequences in schema public to blog_server;

-- 

\c blog

create TABLE users
(
    id serial primary key,
    login varchar(20),
    email varchar(255) not null,
    password varchar(30) not null,
    full_name varchar(70) not null,
    about_me text,
	password_version smallint not null default(0),
	avatar_file_name varchar(14),
	post_count integer not null default(0),
	follower_count integer not null default(0)
);

create or replace function on_password_update()
returns trigger as $$
begin
	new.password_version := case when new.password_version = 32767 then 0 else new.password_version + 1 end;
	return new;
end;
$$ language plpgsql;

create trigger password_update_trigger
before update on users
for each row
when (new.password <> old.password)
execute procedure on_password_update();

-- FOLLOWERS --

create table followers
(
	id serial primary key,
	follower_user_id integer not null references users(id) ON DELETE CASCADE,
	target_user_id integer not null references users(id) ON DELETE CASCADE
);

CREATE or replace function on_follower_add()
returns trigger as $$
begin
	update users set follower_count = follower_count + 1 where id = new.target_user_id;
	return new;
end;
$$ language plpgsql;

CREATE or replace function on_follower_delete()
returns trigger as $$
begin
	update users set follower_count = follower_count - 1 where id = new.target_user_id;
	return null;
end;
$$ language plpgsql;

create trigger follower_add_trigger
after insert on followers
for each row
execute procedure on_follower_add();

create trigger follower_delete_trigger
after delete on followers
for each row
execute procedure on_follower_delete();

-- POSTS --

create table posts
(
    id serial primary key,
	user_id integer not null references users(id) ON DELETE CASCADE,
	body text not null,
	creation_date timestamp not null default(now()),
	comment_count integer not null default(0)
);

create index posts_user_id_index on posts(user_id);

CREATE or replace function on_post_add()
returns trigger as $$
begin
	update users set post_count = post_count + 1 where id = new.user_id;
	return new;
end;
$$ language plpgsql;

CREATE or replace function on_post_delete()
returns trigger as $$
begin
	update users set post_count = post_count - 1 where id = old.user_id;
	return null;
end;
$$ language plpgsql;

create trigger post_add_trigger
after insert on posts
for each row
execute procedure on_post_add();

create trigger post_delete_trigger
after delete on posts
for each row
execute procedure on_post_delete();

-- COMMENTS --

create table comments
(
    id serial primary key,
	post_id integer not null references posts(id) ON DELETE CASCADE,
	user_id integer not null references users(id) ON DELETE CASCADE,
	body text not null,
	creation_date timestamp not null default(now())
);

create index comments_post_id_index on comments(post_id);

CREATE or replace function on_comment_add()
returns trigger as $$
begin
	update posts set comment_count = comment_count + 1 where id = new.post_id;
	return new;
end;
$$ language plpgsql;

CREATE or replace function on_comment_delete()
returns trigger as $$
begin
	update posts set comment_count = comment_count - 1 where id = old.post_id;
	return null;
end;
$$ language plpgsql;

create trigger comment_add_trigger
after insert on comments
for each row
execute procedure on_comment_add();

create trigger comment_delete_trigger
after delete on comments
for each row
execute procedure on_comment_delete();