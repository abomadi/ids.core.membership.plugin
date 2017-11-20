// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
namespace ids.core.membership.plugin.Interfaces
{
    using System.Threading.Tasks;

    public interface IRoleService
    {
        Task<string[]> GetRolesForUser(string userName);
    }
}
